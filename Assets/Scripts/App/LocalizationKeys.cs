namespace ChopChop.App
{
    public static class LocalizationKeys
    {
        public static class App
        {
            public const string Title = "app.title";
            public const string Start = "app.start";
            public const string LevelSelect = "app.levelSelect";
            public const string Language = "app.language";
            public const string Quit = "app.quit";
            public const string Back = "app.back";
            public const string MenuSubtitle = "app.menuSubtitle";
            public const string Settings = "app.settings";
        }

        public static class Settings
        {
            public const string Title = "settings.title";
            public const string Language = "settings.language";
            public const string Japanese = "settings.japanese";
            public const string English = "settings.english";
            public const string Korean = "settings.korean";
            public const string ChineseSimplified = "settings.chineseSimplified";
            public const string French = "settings.french";
            public const string Spanish = "settings.spanish";
        }

        public static class Level
        {
            public const string Locked = "level.locked";
            public const string SetupMissing = "level.setupMissing";
        }

        public static class Hud
        {
            public const string Calibrate = "hud.calibrate";
            public const string ConnectHashi = "hud.connectHashi";
            public const string ResetHashi = "hud.resetHashi";
            public const string Time = "hud.time";
            public const string Failures = "hud.failures";
            public const string Speed = "hud.speed";
            public const string Care = "hud.care";
        }

        public static class Step
        {
            public const string MixEgg = "step.mixEgg";
            public const string PickMeat = "step.pickMeat";
            public const string PutInBroth = "step.putInBroth";
            public const string FlipMeat = "step.flipMeat";
            public const string DipEgg = "step.dipEgg";
            public const string Deliver = "step.deliver";
        }

        public static class Result
        {
            public const string Title = "result.title";
            public const string Retry = "result.retry";
            public const string LevelSelect = "result.levelSelect";
            public const string PlayerOne = "result.playerOne";
            public const string PlayerTwo = "result.playerTwo";
            public const string Winner = "result.winner";
            public const string Draw = "result.draw";
            public const string Total = "result.total";
        }

        public static class Feedback
        {
            public const string Nice = "feedback.nice";
            public const string Failed = "feedback.failed";
            public const string Reset = "feedback.reset";
        }
    }
}
