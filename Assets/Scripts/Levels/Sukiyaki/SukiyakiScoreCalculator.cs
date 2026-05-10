using Mathf = UnityEngine.Mathf;

namespace ChopChop.Levels.Sukiyaki
{
    public static class SukiyakiScoreCalculator
    {
        public static int Speed(float seconds)
        {
            var normalized = Mathf.InverseLerp(75f, 30f, seconds);
            return Mathf.RoundToInt(Mathf.Lerp(45f, 100f, normalized));
        }

        public static int Care(int failureCount, float roughness)
        {
            return Care(failureCount, roughness, 1f);
        }

        public static int Care(int failureCount, float roughness, float stepQuality01)
        {
            var qualityPenalty = (1f - Mathf.Clamp01(stepQuality01)) * 32f;
            var penalty = failureCount * 18f + Mathf.Clamp01(roughness) * 28f + qualityPenalty;
            return Mathf.Clamp(Mathf.RoundToInt(100f - penalty), 0, 100);
        }
    }
}
