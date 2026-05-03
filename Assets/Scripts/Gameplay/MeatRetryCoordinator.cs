using ChopChop.Food.Meat;
using UnityEngine;
using UnityEngine.Events;

namespace ChopChop.Gameplay
{
    [AddComponentMenu("ChopChop/Gameplay/Meat Retry Coordinator")]
    public sealed class MeatRetryCoordinator : MonoBehaviour
    {
        [SerializeField] private MeatTearController tearController;
        [SerializeField] private GameObject meatRoot;
        [SerializeField] private Transform retryStartPoint = null;
        [SerializeField] private UnityEvent onRetryRequested = new UnityEvent();

        public int RetryCount { get; private set; }

        private void Reset()
        {
            tearController = GetComponentInChildren<MeatTearController>();
            if (tearController != null)
            {
                meatRoot = tearController.gameObject;
            }
        }

        private void OnEnable()
        {
            if (tearController != null)
            {
                tearController.Torn += HandleTorn;
            }
        }

        private void OnDisable()
        {
            if (tearController != null)
            {
                tearController.Torn -= HandleTorn;
            }
        }

        private void HandleTorn(MeatTearEvent tearEvent)
        {
            RetryCount++;

            if (meatRoot != null && retryStartPoint != null)
            {
                meatRoot.transform.SetPositionAndRotation(retryStartPoint.position, retryStartPoint.rotation);
            }

            onRetryRequested?.Invoke();
        }
    }
}
