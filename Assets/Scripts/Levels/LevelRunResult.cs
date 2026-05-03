namespace ChopChop.Levels
{
    public readonly struct LevelRunResult
    {
        public float CompletionTime { get; }
        public int FailureCount { get; }
        public int SpeedScore { get; }
        public int CareScore { get; }
        public int TotalScore => SpeedScore + CareScore;
        public bool IsComplete { get; }

        public LevelRunResult(float completionTime, int failureCount, int speedScore, int careScore, bool isComplete)
        {
            CompletionTime = completionTime;
            FailureCount = failureCount;
            SpeedScore = speedScore;
            CareScore = careScore;
            IsComplete = isComplete;
        }
    }
}
