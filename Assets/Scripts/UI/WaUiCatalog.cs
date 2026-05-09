using ChopChop.App;
using UnityEngine;

namespace ChopChop.UI
{
    public static class WaUiCatalog
    {
        private const string Root = "UI/Wa/";
        private const string LabelRoot = Root + "Labels/";

        public static Sprite StartBackground => Load(Root + "background_start");
        public static Sprite LevelSelectBackground => Load(Root + "background_level_select");
        public static Sprite ResultBackground => Load(Root + "background_result");
        public static Sprite SukiyakiStageBackground => Load(Root + "background_sukiyaki_stage");
        public static Sprite MainFrame => Load(Root + "frame_main");
        public static Sprite HeaderPlaque => Load(Root + "header_plaque");
        public static Sprite ResultPanel => Load(Root + "panel_result");
        public static Sprite LevelCard => Load(Root + "level_card");
        public static Sprite SukiyakiHudPanel => Load(Root + "level_card_sukiyaki_hud");
        public static Sprite ButtonPrimary => Load(Root + "button_primary");
        public static Sprite ButtonPrimaryHover => Load(Root + "button_primary_hover");
        public static Sprite ButtonPrimaryPressed => Load(Root + "button_primary_pressed");
        public static Sprite ButtonSecondary => Load(Root + "button_secondary");
        public static Sprite ButtonSecondaryHover => Load(Root + "button_secondary_hover");
        public static Sprite ButtonSecondaryPressed => Load(Root + "button_secondary_pressed");
        public static Sprite ButtonSmall => Load(Root + "button_small");
        public static Sprite ButtonSmallHover => Load(Root + "button_small_hover");
        public static Sprite ButtonSmallPressed => Load(Root + "button_small_pressed");

        public static Sprite LoadLabel(GameLocalization localization, string key)
        {
            var languageFolder = localization.CurrentLanguage == AppLanguage.Japanese ? "ja/" : "en/";
            return Load(LabelRoot + languageFolder + key);
        }

        public static Sprite LoadLocalizedKey(GameLocalization localization, string localizationKey)
        {
            if (string.IsNullOrWhiteSpace(localizationKey))
            {
                return null;
            }

            switch (localizationKey)
            {
                case LocalizationKeys.App.Title:
                    return LoadLabel(localization, "app_title");
                case LocalizationKeys.App.Start:
                    return LoadLabel(localization, "start");
                case LocalizationKeys.App.Language:
                    return LoadLabel(localization, "language");
                case LocalizationKeys.App.Quit:
                    return LoadLabel(localization, "quit");
                case LocalizationKeys.App.Back:
                    return LoadLabel(localization, "back");
                case LocalizationKeys.App.LevelSelect:
                case LocalizationKeys.Result.LevelSelect:
                    return LoadLabel(localization, "level_select");
                case LocalizationKeys.Result.Title:
                    return LoadLabel(localization, "result");
                case LocalizationKeys.Result.Retry:
                    return LoadLabel(localization, "retry");
                case LocalizationKeys.Level.Locked:
                    return LoadLabel(localization, "locked");
                case LocalizationKeys.Hud.Time:
                    return LoadLabel(localization, "hud_time");
                case LocalizationKeys.Hud.Speed:
                    return LoadLabel(localization, "hud_speed");
                case LocalizationKeys.Hud.Care:
                    return LoadLabel(localization, "hud_care");
                case LocalizationKeys.Hud.Failures:
                    return LoadLabel(localization, "hud_failures");
                default:
                    return LoadLabel(localization, SanitizeKey(localizationKey));
            }
        }

        private static Sprite Load(string resourcePath)
        {
            return Resources.Load<Sprite>(resourcePath);
        }

        private static string SanitizeKey(string key)
        {
            return key.Replace(".", "_").Replace(" ", "_");
        }
    }
}
