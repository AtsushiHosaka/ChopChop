using ChopChop.App;

namespace ChopChop.Levels
{
    public static class LevelResultStore
    {
        public static LevelRunResult CurrentResult { get; private set; } = new LevelRunResult(0f, 0, 0, 0, false);
        public static string RetrySceneName { get; private set; } = AppScenes.LevelSelect;

        public static void Set(LevelRunResult result, string retrySceneName)
        {
            CurrentResult = result;
            RetrySceneName = string.IsNullOrWhiteSpace(retrySceneName)
                ? AppScenes.LevelSelect
                : retrySceneName;
        }
    }
}
