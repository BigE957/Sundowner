using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Content.Sources;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Sundowner.Common.Systems.DynamicMusic;

/// <summary>
/// Base class for dynamic music systems that use a LayeredOGGTrack.
/// Handles crossfading between layers based on a desired layer index.
/// </summary>
public abstract class LayeredDynamicMusic : ModSystem
{
    protected LayeredOGGTrack LayeredTrack;
    protected int MusicSlot = -1;

    private int _currentLayerIndex = 0;

    private bool _isTransitioning = false;
    private int _fromLayerIndex;
    private int _toLayerIndex;
    private float _transitionProgress = 0f;
    protected const int TRANSITION_DURATION = 30;

    private int _preFadeDelay = 0;
    protected const int PRE_FADE_DELAY_TICKS = 3;

    /// <summary>
    /// Override to provide the list of layer file paths (relative to mod, with extension).
    /// The order of paths determines layer indices.
    /// </summary>
    protected abstract string[] GetLayerPaths();

    /// <summary>
    /// Override to return the index of the layer that should be active (0‑based).
    /// </summary>
    protected abstract int GetDesiredLayerIndex();

    /// <summary>
    /// Override to provide the music asset path (without extension) for the slot.
    /// This is used to get a music slot ID
    /// By default, this will be the same as the first layer's path without .ogg.
    /// </summary>
    protected virtual string GetMusicAssetPath() => "";

    /// <summary>
    /// Called when starting a transition to a new layer.
    /// </summary>
    protected virtual void OnTransitionStart(int fromLayer, int toLayer) { }

    /// <summary>
    /// Called when a transition completes.
    /// </summary>
    protected virtual void OnTransitionComplete(int newLayer) { }

    public sealed override void PostAddRecipes()
    {
        string[] layerPaths = GetLayerPaths();
        byte[][] layerBytes = new byte[layerPaths.Length][];

        // Load each layer file into a byte array
        for (int i = 0; i < layerPaths.Length; i++)
        {
            using Stream stream = Mod.GetFileStream(layerPaths[i]);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            layerBytes[i] = ms.ToArray();
        }

        // Create layered track with all layers
        LayeredTrack = new LayeredOGGTrack(layerBytes);

        // Set initial layer to full volume, others silent
        for (int i = 0; i < layerPaths.Length; i++)
            LayeredTrack.SetLayerVolume(i, i == _currentLayerIndex ? 1f : 0f);

        string path = GetMusicAssetPath();
        if (path == "")
            path = layerPaths[0].Remove(layerPaths[0].Length - 4);
        MusicSlot = MusicLoader.GetMusicSlot(SundownerMod.Instance, path);

        if (Main.audioSystem is LegacyAudioSystem audioSystem)
        {
            audioSystem.AudioTracks[MusicSlot]?.Dispose();
            audioSystem.AudioTracks[MusicSlot] = LayeredTrack;
        }

        PostMusicLoad();
    }

    protected virtual void PostMusicLoad() { }

    private void StartTransition(int newLayerIndex)
    {
        if (_isTransitioning || _preFadeDelay > 0 || newLayerIndex == _currentLayerIndex)
            return;

        _fromLayerIndex = _currentLayerIndex;
        _toLayerIndex = newLayerIndex;

        OnTransitionStart(_fromLayerIndex, _toLayerIndex);

        _preFadeDelay = PRE_FADE_DELAY_TICKS;
        _isTransitioning = false;
        _transitionProgress = 0f;
    }

    public sealed override void PostUpdateEverything()
    {
        if (!CanUpdate())
            return;

        int desiredLayer = GetDesiredLayerIndex();
        if (desiredLayer != _currentLayerIndex)
            StartTransition(desiredLayer);

        if (_preFadeDelay > 0)
        {
            if (_preFadeDelay == PRE_FADE_DELAY_TICKS)
            {
                for (int i = 0; i < LayeredTrack.LayerCount; i++)
                {
                    float vol = (i == _fromLayerIndex) ? 1f : 0f;
                    LayeredTrack.SetLayerVolume(i, vol);
                }
            }
            _preFadeDelay--;
            if (_preFadeDelay == 0)
            {
                _isTransitioning = true;
                _transitionProgress = 0f;
            }
        }

        if (_isTransitioning)
        {
            _transitionProgress += 1f / TRANSITION_DURATION;

            OnTransitionProgress(_fromLayerIndex, _toLayerIndex, _transitionProgress);

            if (_transitionProgress >= 1f)
            {
                _transitionProgress = 1f;
                _isTransitioning = false;

                _currentLayerIndex = _toLayerIndex;
                OnTransitionComplete(_currentLayerIndex);
            }

            float t = _transitionProgress;
            float fromVol = (float)Math.Sqrt(1f - t); // Approximates cos
            float toVol = (float)Math.Sqrt(t);        // Approximates sin

            for (int i = 0; i < LayeredTrack.LayerCount; i++)
            {
                float vol = 0f;
                if (i == _fromLayerIndex) vol = fromVol;
                else if (i == _toLayerIndex) vol = toVol;
                LayeredTrack.SetLayerVolume(i, vol);
            }
        }

        PostUpdate();
    }

    /// <summary>
    /// Override to conditionally skip updates (e.g., when not in the right biome).
    /// </summary>
    protected virtual bool CanUpdate() => true;

    protected virtual void PostUpdate() { }

    /// <summary>
    /// Called every frame during a transition. 
    /// Use for visual effects, screen shakes, etc.
    /// </summary>
    /// <param name="progress">0.0 to 1.0</param>
    protected virtual void OnTransitionProgress(int fromLayer, int toLayer, float progress) { }

    public override void Unload()
    {
        LayeredTrack?.Dispose();
        LayeredTrack = null;
    }
}