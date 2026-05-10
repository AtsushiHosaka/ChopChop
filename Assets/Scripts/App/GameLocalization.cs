using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace ChopChop.App
{
    public sealed class GameLocalization
    {
        private const string PlayerPrefsKey = "ChopChop.Language";
        private const string StringTableName = "ChopChopText";
        private readonly HashSet<string> warnedMissingKeys = new HashSet<string>();

        public AppLanguage CurrentLanguage { get; private set; }
        public event Action LanguageChanged;

        public GameLocalization()
        {
            CurrentLanguage = (AppLanguage)PlayerPrefs.GetInt(PlayerPrefsKey, 0);
            ApplyLocale();
        }

        public void SetLanguage(AppLanguage language)
        {
            if (CurrentLanguage == language)
            {
                return;
            }

            CurrentLanguage = language;
            PlayerPrefs.SetInt(PlayerPrefsKey, (int)language);
            ApplyLocale();
            LanguageChanged?.Invoke();
        }

        public string Text(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            // Unity StringTable はja/enのロケールアセットのみ存在するため、それ以外は直接カタログを参照する
            if (CurrentLanguage == AppLanguage.Japanese || CurrentLanguage == AppLanguage.English)
            {
                try
                {
                    var text = LocalizationSettings.StringDatabase.GetLocalizedString(StringTableName, key);
                    if (!string.IsNullOrEmpty(text) && text != key)
                    {
                        return text;
                    }
                }
                catch (Exception exception)
                {
                    WarnMissingKey(key, exception.Message);
                }
            }

            return FallbackText(key);
        }

        private string FallbackText(string key)
        {
            var table = CurrentLanguage switch
            {
                AppLanguage.English           => LocalizationCatalog.English,
                AppLanguage.Korean            => LocalizationCatalog.Korean,
                AppLanguage.ChineseSimplified => LocalizationCatalog.ChineseSimplified,
                AppLanguage.French            => LocalizationCatalog.French,
                AppLanguage.Spanish           => LocalizationCatalog.Spanish,
                _                             => LocalizationCatalog.Japanese
            };
            if (table.TryGetValue(key, out var text))
            {
                return text;
            }

            WarnMissingKey(key, "No fallback entry.");
            return key;
        }

        private void ApplyLocale()
        {
            try
            {
                var operation = LocalizationSettings.InitializationOperation;
                if (!operation.IsDone)
                {
                    operation.WaitForCompletion();
                }

                var localeCode = CurrentLanguage switch
                {
                    AppLanguage.Korean            => "ko",
                    AppLanguage.ChineseSimplified => "zh-Hans",
                    AppLanguage.French            => "fr",
                    AppLanguage.Spanish           => "es",
                    AppLanguage.English           => "en",
                    _                             => "ja"
                };
                var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
                if (locale != null)
                {
                    LocalizationSettings.SelectedLocale = locale;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Localization] Could not apply locale: {exception.Message}");
            }
        }

        private void WarnMissingKey(string key, string message)
        {
            if (!warnedMissingKeys.Add(key))
            {
                return;
            }

            Debug.LogWarning($"[Localization] Missing String Table entry '{key}': {message}");
        }
    }
}
