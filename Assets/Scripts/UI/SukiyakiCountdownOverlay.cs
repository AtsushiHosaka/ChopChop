using System;
using ChopChop.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiCountdownOverlay : MonoBehaviour
    {
        [SerializeField] private float secondsPerImage = 0.82f;

        private readonly string[] labels = { "3", "2", "1" };

        private Action completed;
        private CanvasGroup overlayCanvasGroup;
        private CanvasGroup numberCanvasGroup;
        private Text countText;
        private float startedAt;
        private int shownIndex = -1;
        private bool running;

        public void Begin(Action onCompleted)
        {
            completed = onCompleted;
            CreateVisual();
            startedAt = Time.unscaledTime;
            shownIndex = -1;
            running = true;
            ShowIndex(0);
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            var elapsed = Time.unscaledTime - startedAt;
            var index = Mathf.FloorToInt(elapsed / secondsPerImage);
            if (index >= labels.Length)
            {
                Finish();
                return;
            }

            ShowIndex(index);
            Animate(elapsed - index * secondsPerImage);
        }

        private void CreateVisual()
        {
            var overlay = new GameObject("Countdown Overlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            overlay.transform.SetParent(transform, false);
            UiFactory.Stretch(overlay.GetComponent<RectTransform>(), 0f);

            overlayCanvasGroup = overlay.GetComponent<CanvasGroup>();
            overlayCanvasGroup.blocksRaycasts = true;
            overlayCanvasGroup.interactable = true;

            var overlayImage = overlay.GetComponent<Image>();
            overlayImage.color = new Color(0.05f, 0.025f, 0.02f, 0.26f);

            var numberObject = new GameObject("Countdown Number", typeof(RectTransform), typeof(CanvasGroup));
            numberObject.transform.SetParent(overlay.transform, false);
            var numberRect = numberObject.GetComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0.5f, 0.5f);
            numberRect.anchorMax = new Vector2(0.5f, 0.5f);
            numberRect.pivot = new Vector2(0.5f, 0.5f);
            numberRect.anchoredPosition = new Vector2(0f, 4f);
            numberRect.sizeDelta = new Vector2(420f, 240f);

            numberCanvasGroup = numberObject.GetComponent<CanvasGroup>();

            countText = UiFactory.CreateText(numberObject.transform, "Text", 196, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.97f, 0.94f, 0.9f, 1f));
            UiFactory.Stretch(countText.rectTransform, 0f);

            var shadow = countText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.08f, 0.02f, 0.02f, 0.75f);
            shadow.effectDistance = new Vector2(0f, -8f);

            var outline = countText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.42f, 0.09f, 0.07f, 0.65f);
            outline.effectDistance = new Vector2(1.4f, -1.4f);
        }

        private void ShowIndex(int index)
        {
            if (shownIndex == index || countText == null || index < 0 || index >= labels.Length)
            {
                return;
            }

            shownIndex = index;
            countText.text = labels[index];
            countText.rectTransform.localScale = Vector3.one * 1.14f;
            AudioService.Instance?.PlayCue(AudioCueId.CountdownTick);
        }

        private void Animate(float localElapsed)
        {
            if (countText == null || numberCanvasGroup == null)
            {
                return;
            }

            var t = Mathf.Clamp01(localElapsed / secondsPerImage);
            var eased = 1f - Mathf.Pow(1f - t, 3f);
            countText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.14f, 1f, eased);
            numberCanvasGroup.alpha = t > 0.7f ? Mathf.InverseLerp(1f, 0.7f, t) : 1f;
        }

        private void Finish()
        {
            running = false;
            AudioService.Instance?.PlayCue(AudioCueId.CountdownGo);
            completed?.Invoke();
            completed = null;
            if (overlayCanvasGroup != null)
            {
                Destroy(overlayCanvasGroup.gameObject);
            }
        }
    }
}
