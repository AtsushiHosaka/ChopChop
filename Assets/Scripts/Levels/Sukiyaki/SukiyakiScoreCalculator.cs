using Mathf = UnityEngine.Mathf;

namespace ChopChop.Levels.Sukiyaki
{
    public static class SukiyakiScoreCalculator
    {
        public static int Speed(float seconds)
        {
            var normalized = Mathf.InverseLerp(180f, 60f, seconds);
            return Mathf.RoundToInt(Mathf.Lerp(40f, 100f, normalized));
        }

        public static int Care(int failureCount, float roughness)
        {
            var penalty = failureCount * 18f + roughness * 35f;
            return Mathf.Clamp(Mathf.RoundToInt(100f - penalty), 0, 100);
        }
    }
}
