using System;
using System.Collections.Generic;
using JogoBruxinha.Core.Audio;
using UnityEngine;

namespace JogoBruxinha.Gameplay.Dialogue
{
    // Prepare short, windowed syllables once, rather than cutting a WAV at every letter.
    public sealed class DialogueVoicePlayback : IDisposable
    {
        private sealed class PreparedVoice
        {
            public AudioClip Original;
            public AudioClip Blip;
            public float Pitch;
            public float GainDb;
            public float Duration;
        }

        private readonly AudioSource _source;
        private readonly Dictionary<SoundDataSO, PreparedVoice> _cache = new();
        private double _nextBlipTime;

        public DialogueVoicePlayback(AudioSource source)
        {
            _source = source;
        }

        public void Prepare(SoundDataSO sound)
        {
            if (sound == null || sound.Clip == null) return;
            if (_cache.TryGetValue(sound, out var old) && old.Original == sound.Clip &&
                old.Pitch == sound.Pitch && old.GainDb == sound.VoiceGainDb &&
                old.Duration == sound.VoiceBlipDuration) return;

            // Inspector tuning must not destroy the syllable currently reaching the audio device.
            if (old?.Blip != null && _source != null && _source.isPlaying && _source.clip == old.Blip) return;

            if (old?.Blip != null) UnityEngine.Object.Destroy(old.Blip);
            _cache.Remove(sound);
            AudioClip original = sound.Clip;
            if (original.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                Debug.LogWarning($"Voice {original.name} requires Decompress On Load.", sound);
                return;
            }
            if (original.loadState != AudioDataLoadState.Loaded && !original.LoadAudioData()) return;
            if (original.loadState != AudioDataLoadState.Loaded) return;

            float pitch = Mathf.Clamp(sound.Pitch, 0.1f, 3f);
            float duration = Mathf.Clamp(sound.VoiceBlipDuration, 0.03f, 0.15f);
            int frames = Mathf.Min(original.samples, Mathf.Max(2,
                Mathf.RoundToInt(duration * pitch * original.frequency)));
            if (frames < 2) return;
            var samples = new float[frames * original.channels];
            if (!original.GetData(samples, 0)) return;

            int attack = Mathf.Clamp(Mathf.RoundToInt(0.005f * pitch * original.frequency), 1, frames / 2);
            int release = Mathf.Clamp(Mathf.RoundToInt(0.010f * pitch * original.frequency), 1, frames / 2);
            float peak = 0f;
            for (int frame = 0; frame < frames; frame++)
            {
                float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((float)frame / attack));
                float fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((float)(frames - 1 - frame) / release));
                for (int channel = 0; channel < original.channels; channel++)
                {
                    int index = frame * original.channels + channel;
                    samples[index] *= fadeIn * fadeOut;
                    peak = Mathf.Max(peak, Mathf.Abs(samples[index]));
                }
            }

            float gain = Mathf.Pow(10f, sound.VoiceGainDb / 20f);
            // Keep headroom by reducing the whole blip, never by clipping individual samples.
            if (peak * gain > 0.98f)
            {
                gain = 0.98f / peak;
                Debug.LogWarning($"Voice {original.name}: requested gain exceeds headroom; gain was limited to avoid clipping.", sound);
            }
            for (int i = 0; i < samples.Length; i++) samples[i] *= gain;

            var blip = AudioClip.Create(original.name + " (dialogue blip)", frames,
                original.channels, original.frequency, false);
            blip.SetData(samples, 0);
            _cache[sound] = new PreparedVoice
            {
                Original = original, Blip = blip, Pitch = sound.Pitch,
                GainDb = sound.VoiceGainDb, Duration = sound.VoiceBlipDuration
            };
        }

        public bool TryPlay(SoundDataSO sound, float volume)
        {
            if (sound == null || sound.Clip == null || _source == null ||
                _source.isPlaying || AudioSettings.dspTime < _nextBlipTime) return false;
            Prepare(sound);
            if (!_cache.TryGetValue(sound, out var voice)) return false;
            _source.clip = voice.Blip;
            _source.volume = Mathf.Clamp01(volume * sound.Volume);
            _source.pitch = Mathf.Clamp(sound.Pitch, 0.1f, 3f);
            _source.Play();
            _nextBlipTime = AudioSettings.dspTime + voice.Blip.length / _source.pitch;
            return true;
        }

        public void Dispose()
        {
            if (_source != null) _source.Stop();
            foreach (var voice in _cache.Values)
                if (voice.Blip != null) UnityEngine.Object.Destroy(voice.Blip);
            _cache.Clear();
        }
    }
}
