using System;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    [CreateAssetMenu(menuName = "ChopChop/Food/Meat Tear Profile")]
    public sealed class MeatTearProfile : ScriptableObject
    {
        [Header("Durability")]
        [Tooltip("Damage required before the meat tears due to accumulated rough handling.")]
        [Min(1f)] public float durability = 100f;
        [Tooltip("Damage removed per second while the meat is not under harmful stress.")]
        [Min(0f)] public float recoveryPerSecond = 0f;

        [Header("Grip")]
        [Tooltip("Expected distance between chopstick tips when the meat is held gently.")]
        [Min(0.001f)] public float idealTipSeparation = 0.035f;
        [Tooltip("Distance under the ideal separation that starts counting as compression.")]
        [Min(0.001f)] public float compressionDistanceRange = 0.025f;
        [Tooltip("Pinch value treated as normal pressure. Values above this add compression stress.")]
        [Range(0f, 1f)] public float pinchComfortLimit = 0.75f;

        [Header("Obi Tear Settings")]
        [Tooltip("Value copied to ObiTearableCloth. Higher values make natural physical tearing harder.")]
        [Min(0f)] public float tearResistanceMultiplier = 1000f;
        [Tooltip("Value copied to ObiTearableCloth. Limits how many tear events can occur in one frame.")]
        [Min(1)] public int tearRate = 1;
        [Tooltip("Value copied to ObiTearableCloth. Lower values usually create more coherent tear paths.")]
        [Range(0f, 1f)] public float tearDebilitation = 0.5f;

        [Header("Stress Metrics")]
        public StressMetric poke = new StressMetric(0.28f, 0.85f, 40f);
        public StressMetric shear = new StressMetric(0.18f, 0.65f, 35f);
        public StressMetric twist = new StressMetric(140f, 420f, 0.06f);
        public StressMetric tension = new StressMetric(0.035f, 0.12f, 80f);
        public StressMetric compression = new StressMetric(0.75f, 0.95f, 25f);

        public float EvaluateDamagePerSecond(float pokeValue, float shearValue, float twistValue, float tensionValue, float compressionValue)
        {
            return
                poke.EvaluateDamage(pokeValue) +
                shear.EvaluateDamage(shearValue) +
                twist.EvaluateDamage(twistValue) +
                tension.EvaluateDamage(tensionValue) +
                compression.EvaluateDamage(compressionValue);
        }

        public bool IsCritical(MeatStressFrame frame, out MeatTearReason reason)
        {
            if (poke.IsCritical(frame.Poke))
            {
                reason = MeatTearReason.Poke;
                return true;
            }

            if (shear.IsCritical(frame.Shear))
            {
                reason = MeatTearReason.Shear;
                return true;
            }

            if (twist.IsCritical(frame.Twist))
            {
                reason = MeatTearReason.Twist;
                return true;
            }

            if (tension.IsCritical(frame.Tension))
            {
                reason = MeatTearReason.Tension;
                return true;
            }

            if (compression.IsCritical(frame.Compression))
            {
                reason = MeatTearReason.Compression;
                return true;
            }

            reason = MeatTearReason.Unknown;
            return false;
        }

        [Serializable]
        public struct StressMetric
        {
            [Tooltip("Value allowed without damaging the meat.")]
            [Min(0f)] public float tolerance;
            [Tooltip("Value that tears the meat immediately.")]
            [Min(0f)] public float critical;
            [Tooltip("Damage-per-second multiplier for values above tolerance.")]
            [Min(0f)] public float damageWeight;

            public StressMetric(float tolerance, float critical, float damageWeight)
            {
                this.tolerance = tolerance;
                this.critical = Mathf.Max(tolerance, critical);
                this.damageWeight = damageWeight;
            }

            public float EvaluateDamage(float value)
            {
                if (value <= tolerance || tolerance <= 0f)
                {
                    return 0f;
                }

                var overshoot = (value - tolerance) / tolerance;
                return overshoot * overshoot * damageWeight;
            }

            public bool IsCritical(float value)
            {
                return critical > 0f && value >= critical;
            }

            public float Normalized(float value)
            {
                if (critical <= tolerance)
                {
                    return value > tolerance ? 1f : 0f;
                }

                return Mathf.Clamp01((value - tolerance) / (critical - tolerance));
            }
        }
    }
}
