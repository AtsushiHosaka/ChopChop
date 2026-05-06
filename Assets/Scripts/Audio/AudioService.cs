using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChopChop.Audio
{
    [DisallowMultipleComponent]
    public sealed class AudioService : MonoBehaviour
    {
        private const string ResourceRoot = "Audio/";
        private const string BgmEnabledPrefsKey = "ChopChop.BgmEnabled";

        private static AudioService instance;

        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.32f;
        [SerializeField, Range(0f, 1f)] private float uiVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float gameplayVolume = 0.58f;
        [SerializeField] private float bgmFadeSpeed = 0.55f;

        private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private readonly HashSet<string> warnedMissingClips = new HashSet<string>();

        private AudioSource bgmSource;
        private AudioSource uiSource;
        private AudioSource gameplaySource;
        private AudioBgmId currentBgm = AudioBgmId.None;
        private float targetBgmVolume;
        private bool bgmEnabled = true;

        public static AudioService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<AudioService>();
                }

                return instance;
            }
        }

        public bool BgmEnabled => bgmEnabled;
        public event Action BgmSettingsChanged;

        public void Initialize()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            EnsureSources();
            bgmEnabled = PlayerPrefs.GetInt(BgmEnabledPrefsKey, 1) == 1;
        }

        public void PlayBgm(AudioBgmId bgmId)
        {
            EnsureSources();
            if (bgmId == AudioBgmId.None)
            {
                currentBgm = AudioBgmId.None;
                targetBgmVolume = 0f;
                return;
            }

            if (currentBgm == bgmId && bgmSource.isPlaying)
            {
                targetBgmVolume = EffectiveBgmVolume();
                return;
            }

            var clip = LoadClip(ResourcePathFor(bgmId));
            if (clip == null)
            {
                return;
            }

            currentBgm = bgmId;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = 0f;
            targetBgmVolume = EffectiveBgmVolume();
            bgmSource.Play();
        }

        public void SetBgmEnabled(bool enabled)
        {
            if (bgmEnabled == enabled)
            {
                return;
            }

            bgmEnabled = enabled;
            PlayerPrefs.SetInt(BgmEnabledPrefsKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            targetBgmVolume = currentBgm == AudioBgmId.None ? 0f : EffectiveBgmVolume();

            if (enabled && bgmSource != null && bgmSource.clip != null && !bgmSource.isPlaying)
            {
                bgmSource.Play();
            }

            BgmSettingsChanged?.Invoke();
        }

        public void ToggleBgmEnabled()
        {
            SetBgmEnabled(!bgmEnabled);
        }

        public void PlayCue(AudioCueId cueId)
        {
            EnsureSources();
            var clip = LoadClip(ResourcePathFor(cueId));
            if (clip == null)
            {
                return;
            }

            var source = IsUiCue(cueId) ? uiSource : gameplaySource;
            var volume = IsUiCue(cueId) ? uiVolume : gameplayVolume;
            source.PlayOneShot(clip, VolumeMultiplier(cueId) * volume);
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (bgmSource == null)
            {
                return;
            }

            bgmSource.volume = Mathf.MoveTowards(
                bgmSource.volume,
                targetBgmVolume,
                bgmFadeSpeed * Time.unscaledDeltaTime);

            if (currentBgm == AudioBgmId.None && bgmSource.volume <= 0.001f && bgmSource.isPlaying)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
            }
        }

        private float EffectiveBgmVolume()
        {
            return bgmEnabled ? bgmVolume : 0f;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void EnsureSources()
        {
            bgmSource = EnsureSource(bgmSource, "BGM Source");
            uiSource = EnsureSource(uiSource, "UI SE Source");
            gameplaySource = EnsureSource(gameplaySource, "Gameplay SE Source");
        }

        private AudioSource EnsureSource(AudioSource source, string sourceName)
        {
            if (source != null)
            {
                return source;
            }

            var sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);
            source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            return source;
        }

        private AudioClip LoadClip(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }

            if (clipCache.TryGetValue(resourcePath, out var cachedClip))
            {
                return cachedClip;
            }

            var clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                WarnMissingClip(resourcePath);
                return null;
            }

            clipCache.Add(resourcePath, clip);
            return clip;
        }

        private void WarnMissingClip(string resourcePath)
        {
            if (warnedMissingClips.Add(resourcePath))
            {
                Debug.LogWarning($"[Audio] Missing clip Resources/{resourcePath}.wav", this);
            }
        }

        private static bool IsUiCue(AudioCueId cueId)
        {
            return cueId == AudioCueId.UiHover || cueId == AudioCueId.UiClick;
        }

        private static float VolumeMultiplier(AudioCueId cueId)
        {
            switch (cueId)
            {
                case AudioCueId.UiHover:
                    return 0.42f;
                case AudioCueId.CountdownTick:
                    return 0.75f;
                case AudioCueId.StepChanged:
                    return 0.65f;
                case AudioCueId.StepFailure:
                    return 0.85f;
                case AudioCueId.LevelComplete:
                    return 0.9f;
                default:
                    return 1f;
            }
        }

        private static string ResourcePathFor(AudioBgmId bgmId)
        {
            switch (bgmId)
            {
                case AudioBgmId.Menu:
                    return ResourceRoot + "bgm_menu_loop";
                case AudioBgmId.Sukiyaki:
                    return ResourceRoot + "bgm_sukiyaki_loop";
                default:
                    return string.Empty;
            }
        }

        private static string ResourcePathFor(AudioCueId cueId)
        {
            switch (cueId)
            {
                case AudioCueId.UiHover:
                    return ResourceRoot + "se_ui_hover";
                case AudioCueId.UiClick:
                    return ResourceRoot + "se_ui_click";
                case AudioCueId.CountdownTick:
                    return ResourceRoot + "se_countdown_tick";
                case AudioCueId.CountdownGo:
                    return ResourceRoot + "se_countdown_go";
                case AudioCueId.StepChanged:
                    return ResourceRoot + "se_step_changed";
                case AudioCueId.StepSuccess:
                    return ResourceRoot + "se_step_success";
                case AudioCueId.StepFailure:
                    return ResourceRoot + "se_step_failure";
                case AudioCueId.LevelComplete:
                    return ResourceRoot + "se_level_complete";
                default:
                    return string.Empty;
            }
        }
    }
}
