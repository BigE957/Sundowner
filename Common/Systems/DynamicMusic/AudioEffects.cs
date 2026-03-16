using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sundowner.Common.Systems.DynamicMusic;

/// <summary>
/// Audio effects that can be applied to LayeredOGGTrack
/// These process the mixed audio buffer AFTER layers are combined
/// </summary>
public interface IAudioEffect
{
    /// <summary>
    /// Process audio samples in-place
    /// </summary>
    /// <param name="buffer">Audio samples (-1.0 to 1.0)</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="channels">Number of channels (1 = mono, 2 = stereo)</param>
    void Process(float[] buffer, int sampleRate, int channels);

    /// <summary>
    /// Reset internal state (e.g., when track loops)
    /// </summary>
    void Reset();
}

/// <summary>
/// Changes playback speed/pitch
/// </summary>
public class PlaybackSpeedEffect : IAudioEffect
{
    private readonly LayeredOGGTrack _track;
    private float _speed = 1f;

    public PlaybackSpeedEffect(LayeredOGGTrack track)
    {
        _track = track;
    }

    public float Speed
    {
        get => _speed;
        set
        {
            _speed = MathHelper.Clamp(value, -4f, 4f);
            if (_track != null)
            {
                // Positive speed → normal forward with speed change
                if (_speed >= 0)
                {
                    _track.PlaybackSpeed = _speed;
                    _track.Reverse = false;
                }
                // Negative speed → reverse with speed magnitude
                else
                {
                    _track.PlaybackSpeed = -_speed; // use absolute speed for reverse
                    _track.Reverse = true;
                }
            }
        }
    }

    // IAudioEffect.Process – does nothing because the track handles resampling
    public void Process(float[] buffer, int sampleRate, int channels) { }

    public void Reset() { }
}

public class PitchShiftEffect : IAudioEffect
{
    private float _pitchFactor = 1.0f;

    /// <summary>
    /// Pitch multiplier
    /// 1.0 = normal, 2.0 = one octave up, 0.5 = one octave down
    /// </summary>
    public float PitchFactor
    {
        get => _pitchFactor;
        set => _pitchFactor = MathHelper.Clamp(value, 0.25f, 4.0f);
    }

    public void Process(float[] buffer, int sampleRate, int channels)
    {
        if (Math.Abs(_pitchFactor - 1.0f) < 0.001f)
            return;

        int frameCount = buffer.Length / channels;
        float[] output = new float[buffer.Length];

        for (int frame = 0; frame < frameCount; frame++)
        {
            float sourcePos = frame / _pitchFactor;
            int sourceFrame = (int)sourcePos;
            float frac = sourcePos - sourceFrame;

            for (int ch = 0; ch < channels; ch++)
            {
                int idx = frame * channels + ch;
                int srcIdx1 = sourceFrame * channels + ch;
                int srcIdx2 = (sourceFrame + 1) * channels + ch;

                float sample1 = (srcIdx1 >= 0 && srcIdx1 < buffer.Length) ? buffer[srcIdx1] : 0f;
                float sample2 = (srcIdx2 >= 0 && srcIdx2 < buffer.Length) ? buffer[srcIdx2] : 0f;

                output[idx] = sample1 + frac * (sample2 - sample1);
            }
        }

        Array.Copy(output, buffer, buffer.Length);
    }

    public void Reset() { }
}

/// <summary>
/// Low-pass filter (muffles high frequencies)
/// </summary>
public class LowPassFilterEffect : IAudioEffect
{
    private float _cutoffFrequency = 1000f;
    private float[] _lastOutput;

    /// <summary>
    /// Cutoff frequency in Hz
    /// Frequencies above this are attenuated
    /// </summary>
    public float CutoffFrequency
    {
        get => _cutoffFrequency;
        set => _cutoffFrequency = MathHelper.Clamp(value, 20f, 20000f);
    }

    public void Process(float[] buffer, int sampleRate, int channels)
    {
        if (_lastOutput == null || _lastOutput.Length != channels)
        {
            _lastOutput = new float[channels];
        }

        // Simple one-pole low-pass filter
        float rc = 1.0f / (_cutoffFrequency * 2.0f * MathF.PI);
        float dt = 1.0f / sampleRate;
        float alpha = dt / (rc + dt);

        int frameCount = buffer.Length / channels;

        for (int frame = 0; frame < frameCount; frame++)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int idx = frame * channels + ch;
                _lastOutput[ch] = _lastOutput[ch] + alpha * (buffer[idx] - _lastOutput[ch]);
                buffer[idx] = _lastOutput[ch];
            }
        }
    }

    public void Reset()
    {
        if (_lastOutput != null)
            Array.Clear(_lastOutput, 0, _lastOutput.Length);
    }
}

/// <summary>
/// High-pass filter (removes low frequencies)
/// </summary>
public class HighPassFilterEffect : IAudioEffect
{
    private float _cutoffFrequency = 200f;
    private float[] _lastInput;
    private float[] _lastOutput;

    /// <summary>
    /// Cutoff frequency in Hz
    /// Frequencies below this are attenuated
    /// </summary>
    public float CutoffFrequency
    {
        get => _cutoffFrequency;
        set => _cutoffFrequency = MathHelper.Clamp(value, 20f, 20000f);
    }

    public void Process(float[] buffer, int sampleRate, int channels)
    {
        if (_lastInput == null || _lastInput.Length != channels)
        {
            _lastInput = new float[channels];
            _lastOutput = new float[channels];
        }

        float rc = 1.0f / (_cutoffFrequency * 2.0f * MathF.PI);
        float dt = 1.0f / sampleRate;
        float alpha = rc / (rc + dt);

        int frameCount = buffer.Length / channels;

        for (int frame = 0; frame < frameCount; frame++)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int idx = frame * channels + ch;
                float input = buffer[idx];
                _lastOutput[ch] = alpha * (_lastOutput[ch] + input - _lastInput[ch]);
                _lastInput[ch] = input;
                buffer[idx] = _lastOutput[ch];
            }
        }
    }

    public void Reset()
    {
        if (_lastInput != null)
        {
            Array.Clear(_lastInput, 0, _lastInput.Length);
            Array.Clear(_lastOutput, 0, _lastOutput.Length);
        }
    }
}

/// <summary>
/// Simple reverb effect based on Freeverb algorithm
/// </summary>
public class ReverbEffect : IAudioEffect
{
    private CombFilter[] _combFilters;
    private AllpassFilter[] _allpassFilters;

    private float _wetLevel = 0.3f;
    private float _roomSize = 0.5f;
    private float _damping = 0.5f;
    private int _sampleRate = 0;

    public float WetLevel
    {
        get => _wetLevel;
        set => _wetLevel = MathHelper.Clamp(value, 0f, 1f);
    }

    public float RoomSize
    {
        get => _roomSize;
        set
        {
            _roomSize = MathHelper.Clamp(value, 0f, 1f);
            UpdateRoomSize();
        }
    }

    public void Process(float[] buffer, int sampleRate, int channels)
    {
        if (_combFilters == null || _sampleRate != sampleRate)
        {
            _sampleRate = sampleRate;
            InitializeFilters(sampleRate);
        }

        int frameCount = buffer.Length / channels;

        for (int frame = 0; frame < frameCount; frame++)
        {
            // Average input across channels
            float input = 0f;
            for (int ch = 0; ch < channels; ch++)
            {
                input += buffer[frame * channels + ch];
            }
            input /= channels;

            input *= 0.75f;  // Reduce input gain significantly

            // Run through comb filters in parallel
            float combOut = 0f;
            foreach (var comb in _combFilters)
            {
                combOut += comb.Process(input);
            }

            combOut /= _combFilters.Length;

            // Run through allpass filters in series
            float allpassOut = combOut;
            foreach (var allpass in _allpassFilters)
            {
                allpassOut = allpass.Process(allpassOut);
            }

            // Use constant-power crossfade instead of just adding
            float wet = allpassOut * _wetLevel;
            float dry = 1f - (_wetLevel * 0.5f);  // Reduce dry as wet increases

            // Write back to all channels
            for (int ch = 0; ch < channels; ch++)
            {
                int idx = frame * channels + ch;
                buffer[idx] = buffer[idx] * dry + wet;
            }
        }
    }

    private void InitializeFilters(int sampleRate)
    {
        float scale = sampleRate / 44100f;

        int[] combDelays = new int[]
        {
            (int)(1116 * scale),
            (int)(1188 * scale),
            (int)(1277 * scale),
            (int)(1356 * scale),
            (int)(1422 * scale),
            (int)(1491 * scale),
            (int)(1557 * scale),
            (int)(1617 * scale)
        };

        int[] allpassDelays = new int[]
        {
            (int)(556 * scale),
            (int)(441 * scale),
            (int)(341 * scale),
            (int)(225 * scale)
        };

        _combFilters = new CombFilter[combDelays.Length];
        for (int i = 0; i < combDelays.Length; i++)
        {
            _combFilters[i] = new CombFilter(combDelays[i]);
        }

        _allpassFilters = new AllpassFilter[allpassDelays.Length];
        for (int i = 0; i < allpassDelays.Length; i++)
        {
            _allpassFilters[i] = new AllpassFilter(allpassDelays[i]);
        }

        UpdateRoomSize();
    }

    private void UpdateRoomSize()
    {
        if (_combFilters == null) return;

        // FIX #4: Lower feedback range to prevent runaway gain
        float feedback = 0.7f + _roomSize * 0.25f;  // 0.7 to 0.95 (was 0.5 to 0.98)

        foreach (var comb in _combFilters)
        {
            comb.Feedback = feedback;
            comb.Damping = _damping;
        }
    }

    public void Reset()
    {
        if (_combFilters != null)
        {
            foreach (var comb in _combFilters)
                comb.Clear();
        }
        if (_allpassFilters != null)
        {
            foreach (var allpass in _allpassFilters)
                allpass.Clear();
        }
    }

    private class CombFilter
    {
        private float[] _buffer;
        private int _bufferIndex;
        private float _filterStore;
        public float Feedback { get; set; } = 0.84f;
        public float Damping { get; set; } = 0.2f;

        public CombFilter(int size)
        {
            _buffer = new float[size];
            _bufferIndex = 0;
            _filterStore = 0f;
        }

        public float Process(float input)
        {
            float output = _buffer[_bufferIndex];

            // Damping (low-pass filter)
            _filterStore = (output * (1f - Damping)) + (_filterStore * Damping);

            // Write with feedback
            _buffer[_bufferIndex] = input + (_filterStore * Feedback);

            _bufferIndex = (_bufferIndex + 1) % _buffer.Length;

            return output;
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _bufferIndex = 0;
            _filterStore = 0f;
        }
    }

    private class AllpassFilter
    {
        private float[] _buffer;
        private int _bufferIndex;
        private const float Feedback = 0.5f;

        public AllpassFilter(int size)
        {
            _buffer = new float[size];
            _bufferIndex = 0;
        }

        public float Process(float input)
        {
            float bufOut = _buffer[_bufferIndex];
            float output = -input + bufOut;

            _buffer[_bufferIndex] = input + (bufOut * Feedback);
            _bufferIndex = (_bufferIndex + 1) % _buffer.Length;

            return output;
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _bufferIndex = 0;
        }
    }
}

/// <summary>
/// Echo/delay effect
/// </summary>
public class EchoEffect : IAudioEffect
{
    private CircularBuffer _delayBuffer;
    private float _delayMs = 300f;
    private float _feedback = 0.5f;
    private int _currentSampleRate = 0;

    public float DelayMs
    {
        get => _delayMs;
        set
        {
            _delayMs = MathHelper.Clamp(value, 1f, 2000f);
            _delayBuffer = null;
        }
    }

    public float Feedback
    {
        get => _feedback;
        set => _feedback = MathHelper.Clamp(value, 0f, 0.95f);
    }

    public void Process(float[] buffer, int sampleRate, int channels)
    {
        if (_delayBuffer == null || _currentSampleRate != sampleRate)
        {
            int delaySamples = (int)(sampleRate * (_delayMs / 1000f));
            _delayBuffer = new CircularBuffer(delaySamples);
            _currentSampleRate = sampleRate;
        }

        int frameCount = buffer.Length / channels;

        for (int frame = 0; frame < frameCount; frame++)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int idx = frame * channels + ch;
                float input = buffer[idx];
                float delayed = _delayBuffer.Read();

                _delayBuffer.Write(input + delayed * _feedback);
                buffer[idx] = input + delayed;
            }
        }
    }

    public void Reset()
    {
        _delayBuffer?.Clear();
    }

    private class CircularBuffer
    {
        private float[] _buffer;
        private int _pos = 0;

        public CircularBuffer(int size)
        {
            _buffer = new float[Math.Max(1, size)];
        }

        public void Write(float value)
        {
            _buffer[_pos] = value;
            _pos = (_pos + 1) % _buffer.Length;
        }

        public float Read()
        {
            return _buffer[_pos];
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
        }
    }
}