using ChopChop.Core;

namespace ChopChop.Food.Common
{
    public readonly struct FoodFailureEvent
    {
        public string FoodId { get; }
        public PlayerId PlayerId { get; }
        public string Reason { get; }
        public int FailureCount { get; }
        public float TimeSinceLevelStart { get; }

        public FoodFailureEvent(string foodId, PlayerId playerId, string reason, int failureCount, float timeSinceLevelStart)
        {
            FoodId = foodId;
            PlayerId = playerId;
            Reason = reason;
            FailureCount = failureCount;
            TimeSinceLevelStart = timeSinceLevelStart;
        }
    }
}
