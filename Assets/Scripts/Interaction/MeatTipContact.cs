using ChopChop.Interaction;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    public readonly struct MeatTipContact
    {
        public ChopstickTip Tip { get; }
        public Vector3 WorldPoint { get; }
        public Vector3 PreviousWorldPoint { get; }
        public Vector3 InitialLocalPoint { get; }
        public Vector3 CurrentLocalPoint { get; }
        public Vector3 TipVelocity { get; }
        public float PinchNormalized { get; }
        public float ContactAge { get; }

        public MeatTipContact(
            ChopstickTip tip,
            Vector3 worldPoint,
            Vector3 previousWorldPoint,
            Vector3 initialLocalPoint,
            Vector3 currentLocalPoint,
            Vector3 tipVelocity,
            float pinchNormalized,
            float contactAge)
        {
            Tip = tip;
            WorldPoint = worldPoint;
            PreviousWorldPoint = previousWorldPoint;
            InitialLocalPoint = initialLocalPoint;
            CurrentLocalPoint = currentLocalPoint;
            TipVelocity = tipVelocity;
            PinchNormalized = pinchNormalized;
            ContactAge = contactAge;
        }
    }
}
