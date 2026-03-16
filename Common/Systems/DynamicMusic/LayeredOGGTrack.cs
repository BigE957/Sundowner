using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NVorbis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;

namespace Sundowner.Common.Systems.DynamicMusic;

public class LayeredOGGTrack : ASoundEffectBasedAudioTrack
{
    private class Layer
    {
        public VorbisReader Reader;
        public float Volume = 1f;
        public bool IsEnabled = true;
        public byte[] AudioBytes;
    }

    private readonly List<Layer> _layers = [];
    public int LayerCount => _layers.Count;
    private readonly object _lock = new();
    private bool _isDisposed;
    private bool _skipNextReuse = false;

    private readonly List<IAudioEffect> _effects = [];
    private readonly int _sampleRate;
    private readonly int _channels;
    private long _totalSamples;                  // total interleaved samples of the track

    // Speed control
    private float _currentSpeed = 1f;

    public float PlaybackSpeed
    {
        get { lock (_lock) return _currentSpeed; }
        set { lock (_lock) { _currentSpeed = MathHelper.Clamp(value, 0.1f, 4f); } }
    }

    // Reverse control
    private bool _isReversing = false;
    private float _reversePos;                    // current reverse read position in frames (0 = oldest)

    public bool Reverse
    {
        get { lock (_lock) return _isReversing; }
        set
        {
            lock (_lock)
            {
                if (value && !_isReversing)
                {
                    // Switching to reverse: start from the most recent frame in history
                    _reversePos = _historyFrames - 1;
                    if (_reversePos < 0) _reversePos = 0;
                }
                _isReversing = value;
            }
        }
    }

    // History buffer for reverse mode (30 seconds)
    private readonly float[] _historyBuffer;
    private int _historyWritePos = 0;              // next write position in samples
    private int _historyLengthSamples = 0;          // total valid samples in history
    private int _historyFrames = 0;                  // total valid frames in history

    // Ring buffer for forward resampling
    private readonly float[] _ringBuffer;                  // circular buffer of interleaved samples
    private readonly int _ringBufferSize;                   // total samples in ring buffer
    private int _ringReadPos = 0;                   // index of oldest valid sample
    private int _ringWritePos = 0;                  // next write position
    private int _ringAvailableFrames = 0;           // number of valid frames in ring
    private float _phase = 0f;                        // current read position in frames (for forward)
    private const int RingBufferSeconds = 2;          // buffer size in seconds

    // For reverse seeking (kept from previous implementation, but not used in forward)

    public LayeredOGGTrack(params byte[][] layerBytes)
    {
        if (layerBytes.Length == 0)
            throw new ArgumentException("At least one layer required.");

        using (var testReader = new VorbisReader(new MemoryStream(layerBytes[0]), false))
        {
            _sampleRate = testReader.SampleRate;
            _channels = testReader.Channels;
            AudioChannels audioChannels = _channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo;
            CreateSoundEffect(_sampleRate, audioChannels);
        }

        TimeSpan firstDuration = TimeSpan.Zero;
        for (int i = 0; i < layerBytes.Length; i++)
        {
            var reader = new VorbisReader(new MemoryStream(layerBytes[i]), false);

            if (i == 0)
            {
                firstDuration = reader.TotalTime;
            }
            else
            {
                TimeSpan duration = reader.TotalTime;
                if (Math.Abs((duration - firstDuration).TotalSeconds) > 0.1)
                {
                    SundownerMod.Instance?.Logger.Warn(
                        $"Layer {i} duration mismatch: {duration.TotalSeconds:F2}s vs {firstDuration.TotalSeconds:F2}s");
                }
            }

            _layers.Add(new Layer
            {
                Reader = reader,
                AudioBytes = layerBytes[i]
            });
        }

        _totalSamples = (long)(_layers[0].Reader.TotalTime.TotalSeconds * _sampleRate * _channels);

        // History buffer: 30 seconds
        _historyBuffer = new float[_sampleRate * _channels * 30];
        _ringBufferSize = _sampleRate * _channels * RingBufferSeconds;
        _ringBuffer = new float[_ringBufferSize];
        _ringReadPos = 0;
        _ringWritePos = 0;
        _ringAvailableFrames = 0;
        _phase = 0f;
        _reversePos = 0;
    }

    #region Audio Effects Management (unchanged)
    public void AddEffect(IAudioEffect effect) { lock (_lock) _effects.Add(effect); }
    public void RemoveEffect(IAudioEffect effect) { lock (_lock) _effects.Remove(effect); }
    public void ClearEffects() { lock (_lock) _effects.Clear(); }
    public T GetEffect<T>() where T : IAudioEffect { lock (_lock) return _effects.OfType<T>().FirstOrDefault(); }
    #endregion

    #region Layer Control
    public void SetLayerVolume(int layerIndex, float volume)
    {
        if (layerIndex >= 0 && layerIndex < _layers.Count)
            lock (_lock) { _layers[layerIndex].Volume = MathHelper.Clamp(volume, 0f, 1f); }
    }

    public void SetLayerEnabled(int layerIndex, bool enabled)
    {
        if (layerIndex >= 0 && layerIndex < _layers.Count)
            lock (_lock) { _layers[layerIndex].IsEnabled = enabled; }
    }

    public void SeekAll(TimeSpan position)
    {
        lock (_lock)
        {
            foreach (var layer in _layers)
            {
                bool success = false;
                try
                {
                    layer.Reader.TimePosition = position;
                    success = true;
                }
                catch (Exception ex)
                {
                    SundownerMod.Instance?.Logger.Warn($"Layer seek error at {position.TotalSeconds:F2}s: {ex.Message}");
                }

                if (!success)
                {
                    try
                    {
                        var newReader = new VorbisReader(new MemoryStream(layer.AudioBytes), false);
                        newReader.TimePosition = position;
                        layer.Reader.Dispose();
                        layer.Reader = newReader;
                        success = true;
                    }
                    catch (Exception ex2)
                    {
                        SundownerMod.Instance?.Logger.Error($"Failed to recreate layer and seek: {ex2}");
                        try
                        {
                            var fallbackReader = new VorbisReader(new MemoryStream(layer.AudioBytes), false);
                            layer.Reader.Dispose();
                            layer.Reader = fallbackReader;
                        }
                        catch (Exception ex3)
                        {
                            SundownerMod.Instance?.Logger.Error($"Critical: Cannot recreate layer reader: {ex3}");
                        }
                    }
                }
            }

            // Reset ring buffer state because we've moved the source position
            _ringReadPos = 0;
            _ringWritePos = 0;
            _ringAvailableFrames = 0;
            _phase = 0f;

            // Reset reverse position
            _historyWritePos = 0;
            _historyLengthSamples = 0;
            _historyFrames = 0;
            _reversePos = 0;

            foreach (var effect in _effects)
                effect.Reset();
        }
    }
    #endregion

    #region Track Info

    public TimeSpan GetTimePosition()
    {
        TimeSpan time = TimeSpan.Zero;
        bool allSame = true;
        foreach (Layer layer in _layers)
        {
            if (time == TimeSpan.Zero)
                time = layer.Reader.TimePosition;
            else if (time != layer.Reader.TimePosition)
            {
                allSame = false;
                break;
            }
        }

        if (!allSame)
        {
            float highestVolume = -1f;
            foreach (Layer layer in _layers)
            {
                if (layer.Volume > highestVolume)
                    time = layer.Reader.TimePosition;
            }
        }

        return time;
    }

    public TimeSpan GetTimePosition(int layer) => _layers[layer].Reader.TimePosition;

    #endregion

    public void SkipNextReuse()
    {
        lock (_lock) _skipNextReuse = true;
    }

    public override void Reuse()
    {
        bool shouldSkip;
        lock (_lock)
        {
            shouldSkip = _skipNextReuse;
            if (shouldSkip) _skipNextReuse = false;
        }

        if (shouldSkip) return;

        Stop(AudioStopOptions.Immediate);
        lock (_lock)
        {
            foreach (var layer in _layers)
            {
                layer.Reader?.Dispose();
                layer.Reader = new VorbisReader(new MemoryStream(layer.AudioBytes), false);
            }

            _totalSamples = (long)(_layers[0].Reader.TotalTime.TotalSeconds * _sampleRate * _channels);

            _ringReadPos = 0;
            _ringWritePos = 0;
            _ringAvailableFrames = 0;
            _phase = 0f;

            _historyWritePos = 0;
            _historyLengthSamples = 0;
            _historyFrames = 0;
            _reversePos = 0;

            foreach (var effect in _effects)
                effect.Reset();
        }
    }

    protected override void ReadAheadPutAChunkIntoTheBuffer()
    {
        lock (_lock)
        {
            if (_layers.Count == 0 || _soundEffectInstance?.IsDisposed != false)
                return;

            int outputSamples = _temporaryBuffer.Length; // interleaved samples to produce

            if (_isReversing)
            {
                ProcessReverse(outputSamples);
                return;
            }

            float speed = _currentSpeed; // always positive for forward

            // Ensure ring buffer has enough frames ahead of current phase
            int outputFrames = outputSamples / _channels;
            int framesNeeded = (int)Math.Ceiling(outputFrames * speed) + 10; // safety margin
            while (_ringAvailableFrames < framesNeeded)
            {
                int chunkFrames = Math.Min(4096, _ringBufferSize / _channels);
                float[] temp = new float[chunkFrames * _channels];
                int framesRead = MixLayers(temp, chunkFrames); // reads sequentially from layers
                if (framesRead == 0)
                    break;

                // Append to ring buffer (circular)
                for (int i = 0; i < framesRead * _channels; i++)
                {
                    _ringBuffer[_ringWritePos] = temp[i];
                    _ringWritePos = (_ringWritePos + 1) % _ringBufferSize;
                }
                _ringAvailableFrames += framesRead;
            }

            // Resample from ring buffer using phase accumulator
            float[] outputBuffer = new float[outputSamples];
            float currentPhase = _phase;

            for (int outFrame = 0; outFrame < outputFrames; outFrame++)
            {
                int framePos = (int)currentPhase;
                float frac = currentPhase - framePos;

                // Ensure we don't read beyond available samples
                framePos = framePos % _ringAvailableFrames;
                int nextFramePos = (framePos + 1) % _ringAvailableFrames;

                int basePos1 = (_ringReadPos + framePos * _channels) % _ringBufferSize;
                int basePos2 = (_ringReadPos + nextFramePos * _channels) % _ringBufferSize;

                for (int ch = 0; ch < _channels; ch++)
                {
                    int idx1 = (basePos1 + ch) % _ringBufferSize;
                    int idx2 = (basePos2 + ch) % _ringBufferSize;
                    float s1 = _ringBuffer[idx1];
                    float s2 = _ringBuffer[idx2];
                    outputBuffer[outFrame * _channels + ch] = s1 + frac * (s2 - s1);
                }

                currentPhase += speed;
            }

            // Update phase and consume used frames
            float newPhase = currentPhase;
            int consumedFrames = (int)Math.Floor(newPhase);
            _phase = newPhase - consumedFrames;
            _ringReadPos = (_ringReadPos + consumedFrames * _channels) % _ringBufferSize;
            _ringAvailableFrames -= consumedFrames;

            // Update history (for reverse mode)
            UpdateHistory(outputBuffer);

            // Apply other effects (skip speed controller)
            foreach (var effect in _effects)
            {
                if (effect is PlaybackSpeedEffect)
                    continue;
                effect.Process(outputBuffer, _sampleRate, _channels);
            }

            ConvertAndSubmit(outputBuffer, outputSamples);
        }
    }

    private void ProcessReverse(int outputSamples)
    {
        if (_historyFrames == 0)
        {
            // No history yet – output silence
            ConvertAndSubmit(new float[outputSamples], outputSamples);
            return;
        }

        int outputFrames = outputSamples / _channels;
        float speed = _currentSpeed; // positive (0-4)

        float[] outputBuffer = new float[outputSamples];
        float reversePos = _reversePos; // current frame index in history (0 = oldest, increasing)

        // Precompute oldest sample position in history buffer (in samples)
        int oldestSamplePos = (_historyWritePos - _historyLengthSamples + _historyBuffer.Length) % _historyBuffer.Length;

        for (int outFrame = 0; outFrame < outputFrames; outFrame++)
        {
            // Wrap reversePos to [0, _historyFrames-1] using modulo (looping)
            float wrappedPos = reversePos;
            if (wrappedPos < 0)
                wrappedPos += _historyFrames * (float)Math.Ceiling(-wrappedPos / _historyFrames);
            wrappedPos %= _historyFrames;

            int framePos = (int)wrappedPos;
            float frac = wrappedPos - framePos;
            int nextFrame = (framePos + 1) % _historyFrames;

            int basePos1 = (oldestSamplePos + framePos * _channels) % _historyBuffer.Length;
            int basePos2 = (oldestSamplePos + nextFrame * _channels) % _historyBuffer.Length;

            for (int ch = 0; ch < _channels; ch++)
            {
                int idx1 = (basePos1 + ch) % _historyBuffer.Length;
                int idx2 = (basePos2 + ch) % _historyBuffer.Length;
                float s1 = _historyBuffer[idx1];
                float s2 = _historyBuffer[idx2];
                outputBuffer[outFrame * _channels + ch] = s1 + frac * (s2 - s1);
            }

            // Move reversePos backwards (decrease) at given speed
            reversePos -= speed;
        }

        _reversePos = reversePos;

        // Apply other effects
        foreach (var effect in _effects)
        {
            if (effect is PlaybackSpeedEffect)
                continue;
            effect.Process(outputBuffer, _sampleRate, _channels);
        }

        ConvertAndSubmit(outputBuffer, outputSamples);
    }

    private int MixLayers(float[] targetBuffer, int framesToRead)
    {
        int samplesToRead = framesToRead * _channels;
        Array.Clear(targetBuffer, 0, samplesToRead);

        int maxFramesRead = 0;
        bool anyLayerLooped = false;
        float[] layerBuffer = new float[samplesToRead];

        foreach (var layer in _layers)
        {
            int framesRead = layer.Reader.ReadSamples(layerBuffer, 0, samplesToRead) / _channels;
            if (framesRead < framesToRead)
                anyLayerLooped = true;

            maxFramesRead = Math.Max(maxFramesRead, framesRead);

            for (int i = 0; i < framesRead * _channels; i++)
                targetBuffer[i] += layerBuffer[i] * layer.Volume;
        }

        // If any layer looped, seek all to start and fill remaining
        if (anyLayerLooped)
        {
            foreach (var layer in _layers)
                layer.Reader.SeekTo(0, 0);

            int remainingFrames = framesToRead - maxFramesRead;
            if (remainingFrames > 0)
            {
                int remainingSamples = remainingFrames * _channels;
                foreach (var layer in _layers)
                {
                    int framesRead = layer.Reader.ReadSamples(layerBuffer, 0, remainingSamples) / _channels;
                    for (int i = 0; i < framesRead * _channels; i++)
                        targetBuffer[maxFramesRead * _channels + i] += layerBuffer[i] * layer.Volume;
                }
                maxFramesRead = framesToRead;
            }
        }

        return maxFramesRead;
    }

    private void UpdateHistory(float[] newSamples)
    {
        int samplesToWrite = newSamples.Length;
        for (int i = 0; i < samplesToWrite; i++)
        {
            _historyBuffer[_historyWritePos] = newSamples[i];
            _historyWritePos = (_historyWritePos + 1) % _historyBuffer.Length;
            if (_historyLengthSamples < _historyBuffer.Length)
                _historyLengthSamples++;
        }
        // Update frame count (samplesToWrite is guaranteed to be multiple of _channels)
        _historyFrames = _historyLengthSamples / _channels;
    }

    private void ConvertAndSubmit(float[] finalBuffer, int samplesValid)
    {
        for (int i = 0; i < samplesValid; i++)
        {
            float s = finalBuffer[i];
            if (s < -1f) s = -1f;
            else if (s > 1f) s = 1f;

            short sample = (short)(s * short.MaxValue);
            _bufferToSubmit[i * 2] = (byte)(sample & 0xFF);
            _bufferToSubmit[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        _soundEffectInstance.SubmitBuffer(_bufferToSubmit, 0, samplesValid * 2);
    }

    private void SeekAllSamples(long samplePos)
    {
        if (_totalSamples <= 0)
        {
            SundownerMod.Instance?.Logger.Warn("Cannot seek: total samples is zero or negative.");
            return;
        }

        samplePos = Math.Clamp(samplePos, 0L, _totalSamples - 1);
        TimeSpan time = TimeSpan.FromSeconds((double)samplePos / (_sampleRate * _channels));
        SeekAll(time);
    }

    public override void Dispose()
    {
        if (!_isDisposed)
        {
            lock (_lock)
            {
                foreach (var layer in _layers)
                    layer.Reader?.Dispose();
                _layers.Clear();
                _effects.Clear();
            }
            _isDisposed = true;
        }
    }
}