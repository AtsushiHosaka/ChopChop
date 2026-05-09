using ChopChop.App;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiCueOverlay : MonoBehaviour
    {
        [SerializeField] private Vector2 bannerSize = new Vector2(640f, 92f);
        [SerializeField] private Vector2 bannerOffset = new Vector2(0f, -86f);
        [SerializeField] private float feedbackSeconds = 1.2f;
        [SerializeField] private float popScale = 0.06f;
        [SerializeField] private float popSeconds = 0.22f;

        private static readonly Color SuccessColor = new Color(0.18f, 0.85f, 0.38f, 1f);
        private static readonly Color FailureColor = new Color(1f, 0.55f, 0.12f, 1f);

        private GameLocalization localization;
        private SukiyakiStageController stageController;

        private Text bannerText;
        private CanvasGroup bannerGroup;
        private float bannerShownAt;

        private RectTransform feedbackRoot;
        private Text feedbackText;
        private CanvasGroup feedbackGroup;
        private float feedbackEndsAt;
        private float feedbackAnimTime;
        private bool failureShown;

        public void Initialize(GameLocalization appLocalization, SukiyakiStageController stage)
        {
            localization = appLocalization;
            stageController = stage;

            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = 180;

            CreateBanner();
            CreateFeedback();
            Subscribe();

            if (stageController != null)
            {
                ShowInstruction(stageController.CurrentStep);
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            UpdateBannerPop();
            UpdateFeedback();
        }

        private void CreateBanner()
        {
            var bg = new GameObject("Cue Banner", typeof(RectTransform), typeof(CanvasGroup));
            bg.transform.SetParent(transform, false);

            var rt = bg.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = bannerOffset;
            rt.sizeDelta = bannerSize;

            bannerGroup = bg.GetComponent<CanvasGroup>();
            bannerGroup.blocksRaycasts = false;
            bannerGroup.interactable = false;

            var backdropRect = UiFactory.CreatePanel(bg.transform, "Banner Backdrop",
                new Color(0.06f, 0.02f, 0.01f, 0.52f));
            UiFactory.Stretch(backdropRect, 0f);
            backdropRect.GetComponent<Image>().raycastTarget = false;

            bannerText = UiFactory.CreateText(bg.transform, "Banner Text", 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.97f, 0.90f, 0.78f, 1f));
            UiFactory.Stretch(bannerText.rectTransform, 8f);
            bannerText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.65f);
        }

        private void UpdateBannerPop()
        {
            if (bannerText == null) return;
            var elapsed = Time.unscaledTime - bannerShownAt;
            var pop01 = popSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / popSeconds);
            var pop = Mathf.Sin((1f - pop01) * Mathf.PI * 0.5f) * popScale;
            bannerText.rectTransform.localScale = Vector3.one * (1f + pop);
        }

        private void ShowInstruction(SukiyakiStep step)
        {
            if (bannerText == null) return;
            var text = GetStepText(step);
            bannerText.text = text;
            bannerGroup.alpha = string.IsNullOrEmpty(text) ? 0f : 1f;
            bannerShownAt = Time.unscaledTime;
        }

        private void CreateFeedback()
        {
            var root = new GameObject("Feedback", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);

            feedbackRoot = root.GetComponent<RectTransform>();
            feedbackRoot.anchorMin = new Vector2(0.5f, 0.5f);
            feedbackRoot.anchorMax = new Vector2(0.5f, 0.5f);
            feedbackRoot.pivot = new Vector2(0.5f, 0.5f);
            feedbackRoot.anchoredPosition = new Vector2(0f, 88f);
            feedbackRoot.sizeDelta = new Vector2(420f, 120f);

            feedbackGroup = root.GetComponent<CanvasGroup>();
            feedbackGroup.alpha = 0f;
            feedbackGroup.blocksRaycasts = false;
            feedbackGroup.interactable = false;

            feedbackText = UiFactory.CreateText(root.transform, "Feedback Text", 72, FontStyle.Bold, TextAnchor.MiddleCenter, SuccessColor);
            UiFactory.Stretch(feedbackText.rectTransform, 0f);
            feedbackText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);
        }

        private void UpdateFeedback()
        {
            if (feedbackGroup == null) return;

            var secondsRemaining = feedbackEndsAt - Time.unscaledTime;

            if (secondsRemaining <= 0f)
            {
                feedbackGroup.alpha = Mathf.MoveTowards(feedbackGroup.alpha, 0f, Time.unscaledDeltaTime * 5f);
                return;
            }

            feedbackAnimTime += Time.unscaledDeltaTime;
            var spring = 1f - Mathf.Exp(-9f * feedbackAnimTime) * Mathf.Cos(22f * feedbackAnimTime);
            feedbackRoot.localScale = Vector3.one * Mathf.Max(0.01f, spring);

            feedbackGroup.alpha = Mathf.Clamp01(secondsRemaining / 0.3f);
        }

        private void ShowFeedback(string text, Color color)
        {
            if (feedbackText == null) return;
            feedbackText.text = text;
            feedbackText.color = color;
            feedbackEndsAt = Time.unscaledTime + feedbackSeconds;
            feedbackAnimTime = 0f;
            feedbackGroup.alpha = 1f;
        }

        private void Subscribe()
        {
            if (localization != null)
                localization.LanguageChanged += HandleLanguageChanged;

            if (stageController != null)
            {
                stageController.StepChanged += HandleStepChanged;
                stageController.StepSucceeded += HandleStepSucceeded;
                stageController.StepFailed += HandleStepFailed;
            }
        }

        private void Unsubscribe()
        {
            if (localization != null)
                localization.LanguageChanged -= HandleLanguageChanged;

            if (stageController != null)
            {
                stageController.StepChanged -= HandleStepChanged;
                stageController.StepSucceeded -= HandleStepSucceeded;
                stageController.StepFailed -= HandleStepFailed;
            }
        }

        private void HandleLanguageChanged()
        {
            if (stageController != null) ShowInstruction(stageController.CurrentStep);
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            failureShown = false;
            ShowInstruction(step);
        }

        private void HandleStepSucceeded(SukiyakiStep step)
        {
            if (step == SukiyakiStep.Calibration || step == SukiyakiStep.Result) return;
            if (failureShown) return;
            ShowFeedback(localization.Text(LocalizationKeys.Feedback.Nice), SuccessColor);
        }

        private void HandleStepFailed(SukiyakiStep step)
        {
            if (failureShown) return;
            failureShown = true;
            ShowFeedback(localization.Text(LocalizationKeys.Feedback.Failed), FailureColor);
        }

        private string GetStepText(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.Calibration:    return localization.Text(LocalizationKeys.Hud.Calibrate);
                case SukiyakiStep.MixEgg:         return localization.Text(LocalizationKeys.Step.MixEgg);
                case SukiyakiStep.PickMeat:       return localization.Text(LocalizationKeys.Step.PickMeat);
                case SukiyakiStep.PutInBroth:     return localization.Text(LocalizationKeys.Step.PutInBroth);
                case SukiyakiStep.FlipMeat:       return localization.Text(LocalizationKeys.Step.FlipMeat);
                case SukiyakiStep.DipEgg:         return localization.Text(LocalizationKeys.Step.DipEgg);
                case SukiyakiStep.DeliverToMouth: return localization.Text(LocalizationKeys.Step.Deliver);
                default:                           return string.Empty;
            }
        }
    }
}
