using System;
using ChopChop.Core;
using UnityEngine;

namespace ChopChop.Food.Common
{
    public abstract class FoodInteractionActor : MonoBehaviour
    {
        [SerializeField] private string foodId = "food";
        [SerializeField] private int playerIndex = 0;
        [SerializeField] private FoodInteractionState interactionState = FoodInteractionState.Idle;

        public string FoodId => foodId;
        public PlayerId PlayerId => new PlayerId(playerIndex);
        public FoodInteractionState InteractionState => interactionState;
        public int FailureCount { get; private set; }

        public event Action<FoodInteractionState> StateChanged;
        public event Action<FoodFailureEvent> Failed;

        protected void SetFoodId(string value)
        {
            foodId = string.IsNullOrWhiteSpace(value) ? "food" : value.Trim();
        }

        public void SetState(FoodInteractionState nextState)
        {
            if (interactionState == nextState)
            {
                return;
            }

            interactionState = nextState;
            StateChanged?.Invoke(interactionState);
        }

        public FoodFailureEvent RegisterFailure(string reason, float timeSinceLevelStart)
        {
            FailureCount++;
            SetState(FoodInteractionState.Failed);

            var failureEvent = new FoodFailureEvent(
                foodId,
                PlayerId,
                string.IsNullOrWhiteSpace(reason) ? "Unknown" : reason,
                FailureCount,
                timeSinceLevelStart);

            Failed?.Invoke(failureEvent);
            return failureEvent;
        }
    }
}
