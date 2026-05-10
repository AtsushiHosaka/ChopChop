using ChopChop.App;
using ChopChop.Input;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiCueOverlay : MonoBehaviour
    {
        [SerializeField] private Vector2 bannerSize = new Vector2(640f, 92f);
        [SerializeField] private Vector2 bannerOffset = new Vector2(0f, -86f);
        [SerializeField] private Vector2 resetButtonSize = new Vector2(184f, 54f);
        [SerializeField] private Vector2 resetButtonOffset = new Vector2(-32f, -28f);
        [SerializeField] private float feedbackSeconds = 1.2f;
        [SerializeField] private float popScale = 0.06f;
        [SerializeField] private float popSeconds = 0.22f;

        private static readonly Color SuccessColor = new Color(0.02f, 0.018f, 0.015f, 1f);
        private static readonly Color FailureColor = new Color(1f, 0.55f, 0.12f, 1f);

        private GameLocalization localization;
        private SukiyakiStageController stageController;
        private HashiInputProvider inputProvider;

        private Text bannerText;
        private CanvasGroup bannerGroup;
        private float bannerShownAt;
        private bool showingConnectionPrompt;

        private RectTransform feedbackRoot;
        private Text feedbackText;
        private CanvasGroup feedbackGroup;
        private float feedbackEndsAt;
        private float feedbackAnimTime;
        private bool failureShown;

        private Button resetButton;
        private Text resetButtonText;

        public void Initialize(GameLocalization appLocalization, SukiyakiStageController stage)
        {
            localization = appLocalization;
            stageController = stage;
            inputProvider = stageController != null ? stageController.InputProvider : null;

            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = 180;

            CreateBanner();
            CreateFeedback();
            CreateResetButton();
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
            ResolveInputProvider();
            UpdateConnectionPrompt();
            UpdateResetButton();
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

            bannerText = UiFactory.CreateText(bg.transform, "Banner Text", 50, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.99f, 0.93f, 0.82f, 1f));
            UiFactory.Stretch(bannerText.rectTransform, 8f);
            var outline = bannerText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.72f, 0.14f, 0.10f, 1f);
            outline.effectDistance = new Vector2(3f, -3f);
            var shadow = bannerText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.18f, 0.05f, 0.03f, 0.45f);
            shadow.effectDistance = new Vector2(2f, -2f);
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
            if (showingConnectionPrompt) return;
            SetBannerText(GetStepText(step));
        }

        private void SetBannerText(string text)
        {
            if (bannerText == null) return;
            var visible = !string.IsNullOrEmpty(text);
            if (bannerText.text == text && (bannerGroup.alpha > 0f) == visible)
            {
                return;
            }

            bannerText.text = text;
            bannerGroup.alpha = visible ? 1f : 0f;
            bannerShownAt = Time.unscaledTime;
        }

        private void UpdateConnectionPrompt()
        {
            if (bannerText == null) return;

            var shouldShowPrompt = inputProvider != null && inputProvider.NeedsBleConnectionPrompt;
            if (shouldShowPrompt)
            {
                showingConnectionPrompt = true;
                var text = localization != null
                    ? localization.Text(LocalizationKeys.Hud.ConnectHashi)
                    : "BLEが接続されていません";
                SetBannerText(text);
                return;
            }

            if (!showingConnectionPrompt)
            {
                return;
            }

            showingConnectionPrompt = false;
            if (stageController != null)
            {
                SetBannerText(GetStepText(stageController.CurrentStep));
            }
            else
            {
                SetBannerText(string.Empty);
            }
        }

        private void CreateResetButton()
        {
            resetButton = UiFactory.CreateButton(
                transform,
                "Hashi Reset Button",
                ResetButtonLabel(),
                new Color(0.13f, 0.09f, 0.06f, 0.86f),
                new Color(0.98f, 0.91f, 0.78f, 1f));

            var rt = resetButton.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = resetButtonOffset;
            rt.sizeDelta = resetButtonSize;

            resetButtonText = resetButton.transform.Find("Label")?.GetComponent<Text>();
            if (resetButtonText != null)
            {
                resetButtonText.fontSize = 24;
                resetButtonText.resizeTextForBestFit = true;
                resetButtonText.resizeTextMinSize = 16;
                resetButtonText.resizeTextMaxSize = 24;
            }

            resetButton.onClick.AddListener(HandleResetClicked);
            UpdateResetButton();
        }

        private void UpdateResetButton()
        {
            if (resetButton == null) return;

            resetButton.interactable = inputProvider != null
                && inputProvider.CurrentFrame.HasSample
                && !inputProvider.NeedsBleConnectionPrompt;
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
            feedbackText.resizeTextForBestFit = true;
            feedbackText.resizeTextMinSize = 28;
            feedbackText.resizeTextMaxSize = 72;
            UiFactory.Stretch(feedbackText.rectTransform, 0f);
            var shadow = feedbackText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(1f, 0.94f, 0.78f, 0.62f);
            shadow.effectDistance = new Vector2(3f, -3f);
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
            if (resetButtonText != null)
            {
                resetButtonText.text = ResetButtonLabel();
            }

            if (showingConnectionPrompt)
            {
                showingConnectionPrompt = false;
                UpdateConnectionPrompt();
                return;
            }

            if (stageController != null) ShowInstruction(stageController.CurrentStep);
        }

        private void HandleResetClicked()
        {
            ResolveInputProvider();

            if (inputProvider != null && inputProvider.ResetPose())
            {
                showingConnectionPrompt = false;
                if (stageController != null)
                {
                    SetBannerText(GetStepText(stageController.CurrentStep));
                }
                return;
            }

            ShowFeedback(LocalizedText(LocalizationKeys.Hud.ConnectHashi, "BLEが接続されていません"), FailureColor);
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
            var feedbackKey = stageController != null && stageController.LastStepCompletedAutomatically
                ? LocalizationKeys.Feedback.Next
                : LocalizationKeys.Feedback.Nice;
            var fallback = feedbackKey == LocalizationKeys.Feedback.Next ? "次へ！" : "いいね！";
            ShowFeedback(LocalizedText(feedbackKey, fallback), SuccessColor);
        }

        private void HandleStepFailed(SukiyakiStep step)
        {
            if (failureShown) return;
            failureShown = true;
            ShowFeedback(LocalizedText(LocalizationKeys.Feedback.Failed, "ミス！"), FailureColor);
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

        private void ResolveInputProvider()
        {
            if (inputProvider != null)
            {
                return;
            }

            inputProvider = stageController != null
                ? stageController.InputProvider
                : FindAnyObjectByType<HashiInputProvider>();
        }

        private string ResetButtonLabel()
        {
            return LocalizedText(LocalizationKeys.Hud.ResetHashi, "リセット");
        }

        private string LocalizedText(string key, string fallback)
        {
            var text = localization != null ? localization.Text(key) : fallback;
            if (string.IsNullOrWhiteSpace(text) || text == key || text.IndexOf("no translation", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return fallback;
            }

            return text;
        }
    }
}
