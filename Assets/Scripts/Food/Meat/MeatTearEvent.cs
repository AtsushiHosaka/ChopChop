using UnityEngine;

namespace ChopChop.Food.Meat
{
    public readonly struct MeatTearEvent
    {
        public MeatTearReason Reason { get; }
        public MeatStressFrame StressFrame { get; }
        public float Damage { get; }
        public Vector3 WorldPoint { get; }
        public bool WasPhysicalTear { get; }

        public MeatTearEvent(MeatTearReason reason, MeatStressFrame stressFrame, float damage, Vector3 worldPoint, bool wasPhysicalTear)
        {
            Reason = reason;
            StressFrame = stressFrame;
            Damage = damage;
            WorldPoint = worldPoint;
            WasPhysicalTear = wasPhysicalTear;
        }
    }
}
