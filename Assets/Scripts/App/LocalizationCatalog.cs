using System.Collections.Generic;

namespace ChopChop.App
{
    public static class LocalizationCatalog
    {
        private static readonly IReadOnlyDictionary<string, string> JapaneseEntries = new Dictionary<string, string>
        {
            [LocalizationKeys.App.Title] = "ChopChop",
            [LocalizationKeys.App.Start] = "スタート",
            [LocalizationKeys.App.LevelSelect] = "レベル選択",
            [LocalizationKeys.App.Language] = "English",
            [LocalizationKeys.App.Quit] = "終了",
            [LocalizationKeys.App.Back] = "戻る",
            ["level.1.name"] = "Lv.1 卵かけご飯",
            ["level.2.name"] = "Lv.2 豆腐",
            ["level.3.name"] = "Lv.3 刺身",
            ["level.4.name"] = "Lv.4 焼き魚",
            ["level.5.name"] = "Lv.5 すき焼き",
            [LocalizationKeys.Level.Locked] = "準備中",
            ["level.1.desc"] = "やさしく混ぜて食べる練習",
            ["level.2.desc"] = "崩れやすい豆腐をつかむ練習",
            ["level.3.desc"] = "すべりやすい刺身をつかむ練習",
            ["level.4.desc"] = "焼き魚の身をほぐす練習",
            ["level.5.desc"] = "肉を返して卵につける総合練習",
            [LocalizationKeys.Hud.Calibrate] = "箸を自然に持ってください",
            [LocalizationKeys.Step.MixEgg] = "卵をかき混ぜろ！",
            [LocalizationKeys.Step.PickMeat] = "肉をつかめ！",
            [LocalizationKeys.Step.PutInBroth] = "割下に入れろ！",
            [LocalizationKeys.Step.FlipMeat] = "肉を返せ！",
            [LocalizationKeys.Step.DipEgg] = "卵につけろ！",
            [LocalizationKeys.Step.Deliver] = "口へ運べ！",
            [LocalizationKeys.Hud.Time] = "時間",
            [LocalizationKeys.Hud.Failures] = "失敗",
            [LocalizationKeys.Hud.Speed] = "Speed",
            [LocalizationKeys.Hud.Care] = "Care",
            [LocalizationKeys.Level.SetupMissing] = "Lv.5のシーン参照が未設定です",
            [LocalizationKeys.Result.Title] = "結果",
            [LocalizationKeys.Result.Retry] = "もう一度",
            [LocalizationKeys.Result.LevelSelect] = "レベル選択へ"
        };

        private static readonly IReadOnlyDictionary<string, string> EnglishEntries = new Dictionary<string, string>
        {
            [LocalizationKeys.App.Title] = "ChopChop",
            [LocalizationKeys.App.Start] = "Start",
            [LocalizationKeys.App.LevelSelect] = "Level Select",
            [LocalizationKeys.App.Language] = "日本語",
            [LocalizationKeys.App.Quit] = "Quit",
            [LocalizationKeys.App.Back] = "Back",
            ["level.1.name"] = "Lv.1 Tamago Kake Gohan",
            ["level.2.name"] = "Lv.2 Tofu",
            ["level.3.name"] = "Lv.3 Sashimi",
            ["level.4.name"] = "Lv.4 Grilled Fish",
            ["level.5.name"] = "Lv.5 Sukiyaki",
            [LocalizationKeys.Level.Locked] = "Coming Soon",
            ["level.1.desc"] = "Practice gentle mixing and lifting.",
            ["level.2.desc"] = "Practice holding fragile tofu.",
            ["level.3.desc"] = "Practice lifting slippery sashimi.",
            ["level.4.desc"] = "Practice flaking grilled fish.",
            ["level.5.desc"] = "Flip beef and dip it in egg.",
            [LocalizationKeys.Hud.Calibrate] = "Hold the chopsticks naturally.",
            [LocalizationKeys.Step.MixEgg] = "Mix the egg!",
            [LocalizationKeys.Step.PickMeat] = "Pick up the beef!",
            [LocalizationKeys.Step.PutInBroth] = "Put it in the broth!",
            [LocalizationKeys.Step.FlipMeat] = "Flip the beef!",
            [LocalizationKeys.Step.DipEgg] = "Dip it in egg!",
            [LocalizationKeys.Step.Deliver] = "Bring it to the mouth!",
            [LocalizationKeys.Hud.Time] = "Time",
            [LocalizationKeys.Hud.Failures] = "Fails",
            [LocalizationKeys.Hud.Speed] = "Speed",
            [LocalizationKeys.Hud.Care] = "Care",
            [LocalizationKeys.Level.SetupMissing] = "Lv.5 scene references are not assigned.",
            [LocalizationKeys.Result.Title] = "Result",
            [LocalizationKeys.Result.Retry] = "Retry",
            [LocalizationKeys.Result.LevelSelect] = "Level Select"
        };

        public static IReadOnlyDictionary<string, string> Japanese => JapaneseEntries;
        public static IReadOnlyDictionary<string, string> English => EnglishEntries;
    }
}
