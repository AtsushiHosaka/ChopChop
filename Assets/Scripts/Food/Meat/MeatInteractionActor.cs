using System;
using ChopChop.Food.Common;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    [AddComponentMenu("ChopChop/Food/Meat Interaction Actor")]
    public sealed class MeatInteractionActor : FoodInteractionActor
    {
        [SerializeField] private MeatState meatState = MeatState.Idle;

        public MeatState MeatState => meatState;

        public event Action<MeatState> MeatStateChanged;

        public void SetMeatState(MeatState nextState)
        {
            if (meatState == nextState)
            {
                return;
            }

            meatState = nextState;
            SetState(ToInteractionState(nextState));
            MeatStateChanged?.Invoke(meatState);
        }

        private static FoodInteractionState ToInteractionState(MeatState state)
        {
            switch (state)
            {
                case MeatState.Touched:
                    return FoodInteractionState.Touched;
                case MeatState.Gripped:
                case MeatState.FlipReady:
                    return FoodInteractionState.Gripped;
                case MeatState.Flipping:
                case MeatState.Cooked:
                case MeatState.Dipped:
                    return FoodInteractionState.InProgress;
                case MeatState.Delivered:
                    return FoodInteractionState.Completed;
                case MeatState.Torn:
                    return FoodInteractionState.Failed;
                case MeatState.Idle:
                default:
                    return FoodInteractionState.Idle;
            }
        }
    }
}
