using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Slider))]
    public sealed class UiSliderValueAnimator : MonoBehaviour
    {
        [SerializeField] private float smoothSeconds = 0.14f;

        private Slider slider;
        private float targetValue;
        private float displayedValue;
        private float velocity;
        private float lastAppliedValue;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            targetValue = slider.value;
            displayedValue = targetValue;
            lastAppliedValue = targetValue;
        }

        private void LateUpdate()
        {
            if (slider == null)
            {
                return;
            }

            if (!Mathf.Approximately(slider.value, lastAppliedValue))
            {
                targetValue = slider.value;
            }

            displayedValue = Mathf.SmoothDamp(
                displayedValue,
                targetValue,
                ref velocity,
                Mathf.Max(0.01f, smoothSeconds),
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            slider.value = displayedValue;
            lastAppliedValue = displayedValue;
        }
    }
}
