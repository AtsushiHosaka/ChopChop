using UnityEngine;

namespace ChopChop.UI
{
    public static class WaUiCatalog
    {
        private const string Root = "UI/Wa/";

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

        private static Sprite Load(string resourcePath)
        {
            return Resources.Load<Sprite>(resourcePath);
        }
    }
}
