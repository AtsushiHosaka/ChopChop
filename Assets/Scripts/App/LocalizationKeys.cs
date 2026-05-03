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
        }

        public static class Level
        {
            public const string Locked = "level.locked";
            public const string SetupMissing = "level.setupMissing";
        }

        public static class Hud
        {
            public const string Calibrate = "hud.calibrate";
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
        }
    }
}
