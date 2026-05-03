using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChopChop.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Button))]
    public sealed class UiButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.035f;
        [SerializeField] private float pressedScale = 0.965f;
        [SerializeField] private float settleSpeed = 18f;

        private RectTransform rectTransform;
        private Button button;
        private float targetScale = 1f;
        private bool hovered;
        private bool pressed;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            button = GetComponent<Button>();
        }

        private void OnDisable()
        {
            hovered = false;
            pressed = false;
            targetScale = 1f;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            if (rectTransform == null)
            {
                return;
            }

            var current = rectTransform.localScale.x;
            var next = Mathf.Lerp(current, targetScale, 1f - Mathf.Exp(-settleSpeed * Time.unscaledDeltaTime));
            rectTransform.localScale = Vector3.one * next;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            RefreshTargetScale();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
            RefreshTargetScale();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
            RefreshTargetScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
            RefreshTargetScale();
        }

        private void RefreshTargetScale()
        {
            if (button != null && !button.interactable)
            {
                targetScale = 1f;
                return;
            }

            targetScale = pressed ? pressedScale : hovered ? hoverScale : 1f;
        }
    }
}
