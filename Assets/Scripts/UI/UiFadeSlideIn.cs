using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class UiFadeSlideIn : MonoBehaviour
    {
        [SerializeField] private float duration = 0.36f;
        [SerializeField] private float delay;
        [SerializeField] private Vector2 startOffset = new Vector2(0f, -16f);
        [SerializeField] private float startScale = 0.98f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 targetPosition;
        private float startedAt;
        private bool playing;
        private bool animatePosition;

        public void Configure(float animationDuration, float animationDelay, Vector2 offset, float initialScale = 0.98f)
        {
            duration = Mathf.Max(0.01f, animationDuration);
            delay = Mathf.Max(0f, animationDelay);
            startOffset = offset;
            startScale = Mathf.Max(0.01f, initialScale);
        }

        public void Play()
        {
            Resolve();
            targetPosition = rectTransform.anchoredPosition;
            animatePosition = startOffset.sqrMagnitude > 0.001f && !IsLayoutControlled();
            startedAt = Time.unscaledTime;
            playing = true;
            Apply(0f);
        }

        private void OnEnable()
        {
            Play();
        }

        private void Update()
        {
            if (!playing)
            {
                return;
            }

            var elapsed = Time.unscaledTime - startedAt - delay;
            var t = Mathf.Clamp01(elapsed / duration);
            Apply(Smooth(t));

            if (t >= 1f)
            {
                playing = false;
            }
        }

        private void Apply(float t)
        {
            if (animatePosition)
            {
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(targetPosition + startOffset, targetPosition, t);
            }

            rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(startScale, 1f, t);
            canvasGroup.alpha = t;
        }

        private void Resolve()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private bool IsLayoutControlled()
        {
            return transform.parent != null && transform.parent.GetComponent<LayoutGroup>() != null;
        }

        private static float Smooth(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
