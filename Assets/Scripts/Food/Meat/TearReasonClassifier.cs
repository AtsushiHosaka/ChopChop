using System.Collections.Generic;

namespace ChopChop.Food.Meat
{
    public static class TearReasonClassifier
    {
        public static MeatTearReason ClassifyFrame(
            MeatTearProfile profile,
            float poke,
            float shear,
            float twist,
            float tension,
            float compression)
        {
            var bestReason = MeatTearReason.Unknown;
            var bestScore = 0f;

            Consider(MeatTearReason.Poke, profile.poke.Normalized(poke), ref bestReason, ref bestScore);
            Consider(MeatTearReason.Shear, profile.shear.Normalized(shear), ref bestReason, ref bestScore);
            Consider(MeatTearReason.Twist, profile.twist.Normalized(twist), ref bestReason, ref bestScore);
            Consider(MeatTearReason.Tension, profile.tension.Normalized(tension), ref bestReason, ref bestScore);
            Consider(MeatTearReason.Compression, profile.compression.Normalized(compression), ref bestReason, ref bestScore);

            return bestReason;
        }

        public static MeatTearReason ClassifyHistory(IReadOnlyList<MeatStressFrame> frames, MeatTearProfile profile)
        {
            if (profile == null)
            {
                return MeatTearReason.Unknown;
            }

            var scores = new Dictionary<MeatTearReason, float>
            {
                { MeatTearReason.Poke, 0f },
                { MeatTearReason.Shear, 0f },
                { MeatTearReason.Twist, 0f },
                { MeatTearReason.Tension, 0f },
                { MeatTearReason.Compression, 0f }
            };

            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];
                scores[MeatTearReason.Poke] += profile.poke.Normalized(frame.Poke);
                scores[MeatTearReason.Shear] += profile.shear.Normalized(frame.Shear);
                scores[MeatTearReason.Twist] += profile.twist.Normalized(frame.Twist);
                scores[MeatTearReason.Tension] += profile.tension.Normalized(frame.Tension);
                scores[MeatTearReason.Compression] += profile.compression.Normalized(frame.Compression);
            }

            var bestReason = MeatTearReason.Unknown;
            var bestScore = 0f;
            foreach (var pair in scores)
            {
                Consider(pair.Key, pair.Value, ref bestReason, ref bestScore);
            }

            return bestReason;
        }

        private static void Consider(MeatTearReason reason, float score, ref MeatTearReason bestReason, ref float bestScore)
        {
            if (score <= bestScore)
            {
                return;
            }

            bestReason = reason;
            bestScore = score;
        }
    }
}
