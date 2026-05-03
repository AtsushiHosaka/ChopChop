using System;
using ChopChop.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiCountdownOverlay : MonoBehaviour
    {
        private const string ResourceRoot = "UI/Countdown/countdown_";

        [SerializeField] private float secondsPerImage = 0.82f;
        [SerializeField] private Vector2 imageSize = new Vector2(360f, 360f);

        private readonly string[] keys = { "3", "2", "1" };

        private Action completed;
        private CanvasGroup canvasGroup;
        private Image image;
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
            if (index >= keys.Length)
            {
                Finish();
                return;
            }

            ShowIndex(index);
            Animate(elapsed - index * secondsPerImage);
        }

        private void CreateVisual()
        {
            var blocker = new GameObject("Countdown Overlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            blocker.transform.SetParent(transform, false);
            UiFactory.Stretch(blocker.GetComponent<RectTransform>(), 0f);

            canvasGroup = blocker.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            var blockerImage = blocker.GetComponent<Image>();
            blockerImage.color = new Color(0f, 0f, 0f, 0.32f);

            var imageObject = new GameObject("Countdown Image", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(blocker.transform, false);
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = imageSize;

            image = imageObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
        }

        private void ShowIndex(int index)
        {
            if (shownIndex == index || image == null || index < 0 || index >= keys.Length)
            {
                return;
            }

            shownIndex = index;
            var sprite = Resources.Load<Sprite>(ResourceRoot + keys[index]);
            if (sprite == null)
            {
                Debug.LogWarning($"[Countdown] Missing sprite Resources/{ResourceRoot}{keys[index]}.png", this);
                image.enabled = false;
                return;
            }

            image.enabled = true;
            image.sprite = sprite;
            image.rectTransform.localScale = Vector3.one * 1.18f;
            AudioService.Instance?.PlayCue(AudioCueId.CountdownTick);
        }

        private void Animate(float localElapsed)
        {
            if (image == null || canvasGroup == null)
            {
                return;
            }

            var t = Mathf.Clamp01(localElapsed / secondsPerImage);
            var pop = 1f + Mathf.Sin((1f - t) * Mathf.PI * 0.5f) * 0.18f;
            image.rectTransform.localScale = Vector3.one * pop;
            canvasGroup.alpha = t > 0.72f ? Mathf.InverseLerp(1f, 0.72f, t) : 1f;
        }

        private void Finish()
        {
            running = false;
            AudioService.Instance?.PlayCue(AudioCueId.CountdownGo);
            completed?.Invoke();
            completed = null;
            if (canvasGroup != null)
            {
                Destroy(canvasGroup.gameObject);
            }
        }
    }
}
