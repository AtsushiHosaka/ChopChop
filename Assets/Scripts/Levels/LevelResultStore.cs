using ChopChop.App;

namespace ChopChop.Levels
{
    public static class LevelResultStore
    {
        public static LevelRunResult CurrentResult { get; private set; } = new LevelRunResult(0f, 0, 0, 0, false);
        public static VersusLevelRunResult CurrentVersusResult { get; private set; }
        public static bool HasVersusResult { get; private set; }
        public static string RetrySceneName { get; private set; } = AppScenes.LevelSelect;

        public static void Set(LevelRunResult result, string retrySceneName)
        {
            CurrentResult = result;
            CurrentVersusResult = default;
            HasVersusResult = false;
            RetrySceneName = string.IsNullOrWhiteSpace(retrySceneName)
                ? AppScenes.LevelSelect
                : retrySceneName;
        }

        public static void SetVersus(VersusLevelRunResult result, string retrySceneName)
        {
            CurrentResult = result.WinnerPlayerIndex == 1 ? result.PlayerTwo : result.PlayerOne;
            CurrentVersusResult = result;
            HasVersusResult = true;
            RetrySceneName = string.IsNullOrWhiteSpace(retrySceneName)
                ? AppScenes.LevelSelect
                : retrySceneName;
        }
    }
}
