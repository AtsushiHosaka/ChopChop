namespace ChopChop.Levels
{
    public readonly struct VersusLevelRunResult
    {
        public LevelRunResult PlayerOne { get; }
        public LevelRunResult PlayerTwo { get; }
        public bool IsComplete => PlayerOne.IsComplete && PlayerTwo.IsComplete;
        public bool IsDraw => PlayerOne.TotalScore == PlayerTwo.TotalScore;
        public int WinnerPlayerIndex => IsDraw ? -1 : PlayerOne.TotalScore > PlayerTwo.TotalScore ? 0 : 1;

        public VersusLevelRunResult(LevelRunResult playerOne, LevelRunResult playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
        }
    }
}
