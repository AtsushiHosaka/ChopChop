using UnityEngine;

namespace ChopChop.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UiPunchScale : MonoBehaviour
    {
        [SerializeField] private float punchScale = 0.08f;
        [SerializeField] private float duration = 0.24f;

        private RectTransform rectTransform;
        private float startedAt;
        private bool playing;

        public void Play()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            startedAt = Time.unscaledTime;
            playing = true;
        }

        private void OnDisable()
        {
            playing = false;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            if (!playing)
            {
                return;
            }

            var t = Mathf.Clamp01((Time.unscaledTime - startedAt) / Mathf.Max(0.01f, duration));
            var wave = Mathf.Sin((1f - t) * Mathf.PI);
            rectTransform.localScale = Vector3.one * (1f + wave * punchScale);

            if (t >= 1f)
            {
                rectTransform.localScale = Vector3.one;
                playing = false;
            }
        }
    }
}
