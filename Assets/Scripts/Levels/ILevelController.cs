namespace ChopChop.Levels
{
    public interface ILevelController
    {
        bool IsRunning { get; }
        LevelRunResult CurrentResult { get; }
        void StartLevel();
        void RestartLevel();
    }
}
