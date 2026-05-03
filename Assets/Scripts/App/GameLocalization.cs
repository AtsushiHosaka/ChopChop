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
            CurrentLanguage = PlayerPrefs.GetInt(PlayerPrefsKey, 0) == 1
                ? AppLanguage.English
                : AppLanguage.Japanese;
            ApplyLocale();
        }

        public void ToggleLanguage()
        {
            SetLanguage(CurrentLanguage == AppLanguage.Japanese ? AppLanguage.English : AppLanguage.Japanese);
        }

        public void SetLanguage(AppLanguage language)
        {
            if (CurrentLanguage == language)
            {
                return;
            }

            CurrentLanguage = language;
            PlayerPrefs.SetInt(PlayerPrefsKey, language == AppLanguage.English ? 1 : 0);
            ApplyLocale();
            LanguageChanged?.Invoke();
        }

        public string Text(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            try
            {
                var text = LocalizationSettings.StringDatabase.GetLocalizedString(StringTableName, key);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
            catch (Exception exception)
            {
                WarnMissingKey(key, exception.Message);
            }

            return FallbackText(key);
        }

        private string FallbackText(string key)
        {
            var table = CurrentLanguage == AppLanguage.English
                ? LocalizationCatalog.English
                : LocalizationCatalog.Japanese;
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

                var localeCode = CurrentLanguage == AppLanguage.Japanese ? "ja" : "en";
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
