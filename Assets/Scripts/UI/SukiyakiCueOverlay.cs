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

        // 指示バナー（上部）
        private Image bannerLabel;
        private CanvasGroup bannerGroup;
        private float bannerShownAt;

        // フィードバック（中央）
        private RectTransform feedbackRoot;
        private Image feedbackLabel;
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

        // ─── 指示バナー ──────────────────────────────────────

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

            // 半透明背景（指示テキストの可読性向上）
            var backdropRect = UiFactory.CreatePanel(bg.transform, "Banner Backdrop",
                new Color(0.06f, 0.02f, 0.01f, 0.52f));
            UiFactory.Stretch(backdropRect, 0f);
            backdropRect.GetComponent<Image>().raycastTarget = false;

            bannerLabel = UiFactory.CreateSpriteImage(bg.transform, "Banner Label", null, true);
            UiFactory.Stretch(bannerLabel.rectTransform, 0f);
        }

        private void UpdateBannerPop()
        {
            if (bannerLabel == null) return;
            var elapsed = Time.unscaledTime - bannerShownAt;
            var pop01 = popSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / popSeconds);
            var pop = Mathf.Sin((1f - pop01) * Mathf.PI * 0.5f) * popScale;
            bannerLabel.rectTransform.localScale = Vector3.one * (1f + pop);
        }

        private void ShowInstruction(SukiyakiStep step)
        {
            if (bannerLabel == null) return;
            var sprite = LoadStepSprite(step);
            UiFactory.ApplySprite(bannerLabel, sprite);
            bannerGroup.alpha = sprite == null ? 0f : 1f;
            bannerShownAt = Time.unscaledTime;
        }

        // ─── フィードバック（スタイリッシュ大テキスト） ────────

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

            feedbackLabel = UiFactory.CreateSpriteImage(root.transform, "Feedback Label", null, true);
            UiFactory.Stretch(feedbackLabel.rectTransform, 0f);
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

            // スプリングアニメーション（弾む登場）
            feedbackAnimTime += Time.unscaledDeltaTime;
            var spring = 1f - Mathf.Exp(-9f * feedbackAnimTime) * Mathf.Cos(22f * feedbackAnimTime);
            feedbackRoot.localScale = Vector3.one * Mathf.Max(0.01f, spring);

            // フェード：登場は即座、終了前0.3秒でフェードアウト
            feedbackGroup.alpha = Mathf.Clamp01(secondsRemaining / 0.3f);
        }

        private void ShowFeedback(Sprite sprite)
        {
            if (feedbackLabel == null) return;
            UiFactory.ApplySprite(feedbackLabel, sprite);
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
            ShowFeedback(WaUiCatalog.LoadLabel(localization, "feedback_nice"));
        }

        private void HandleStepFailed(SukiyakiStep step)
        {
            if (failureShown) return;
            failureShown = true;
            ShowFeedback(WaUiCatalog.LoadLabel(localization, "feedback_failed"));
        }

        private Sprite LoadStepSprite(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.Calibration:    return WaUiCatalog.LoadLabel(localization, "step_calibration");
                case SukiyakiStep.MixEgg:         return WaUiCatalog.LoadLabel(localization, "step_mix_egg");
                case SukiyakiStep.PickMeat:       return WaUiCatalog.LoadLabel(localization, "step_pick_meat");
                case SukiyakiStep.PutInBroth:     return WaUiCatalog.LoadLabel(localization, "step_put_in_broth");
                case SukiyakiStep.FlipMeat:       return WaUiCatalog.LoadLabel(localization, "step_flip_meat");
                case SukiyakiStep.DipEgg:         return WaUiCatalog.LoadLabel(localization, "step_dip_egg");
                case SukiyakiStep.DeliverToMouth: return WaUiCatalog.LoadLabel(localization, "step_deliver");
                default:                           return null;
            }
        }
    }
}
