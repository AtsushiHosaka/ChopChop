using System.Collections.Generic;
using ChopChop.App;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiCueOverlay : MonoBehaviour
    {
        private const string ResourceRoot = "UI/SukiyakiCues/";
        private const string SuccessKey = "feedback_success";
        private const string FailureKey = "feedback_failure";

        private readonly HashSet<string> warnedMissingSprites = new HashSet<string>();

        [SerializeField] private Vector2 size = new Vector2(940f, 164f);
        [SerializeField] private Vector2 topOffset = new Vector2(0f, -116f);
        [SerializeField] private float feedbackSeconds = 0.85f;
        [SerializeField] private float popScale = 0.08f;
        [SerializeField] private float popSeconds = 0.22f;

        private GameLocalization localization;
        private SukiyakiStageController stageController;
        private Image image;
        private CanvasGroup canvasGroup;
        private string currentInstructionKey;
        private string pendingInstructionKey;
        private float feedbackEndsAt;
        private float shownAt;
        private bool showingFeedback;

        public void Initialize(GameLocalization appLocalization, SukiyakiStageController stage)
        {
            localization = appLocalization;
            stageController = stage;

            CreateVisual();
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
            if (image == null || canvasGroup == null)
            {
                return;
            }

            var elapsed = Time.unscaledTime - shownAt;
            var pop01 = popSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / popSeconds);
            var pop = Mathf.Sin((1f - pop01) * Mathf.PI * 0.5f) * popScale;
            image.rectTransform.localScale = Vector3.one * (1f + pop);

            if (!showingFeedback)
            {
                canvasGroup.alpha = 1f;
                return;
            }

            var secondsRemaining = feedbackEndsAt - Time.unscaledTime;
            canvasGroup.alpha = Mathf.Clamp01(secondsRemaining / 0.18f);
            if (secondsRemaining > 0f)
            {
                return;
            }

            showingFeedback = false;
            ShowInstructionByKey(string.IsNullOrEmpty(pendingInstructionKey) ? currentInstructionKey : pendingInstructionKey);
            pendingInstructionKey = string.Empty;
        }

        private void CreateVisual()
        {
            var cueObject = new GameObject("Cue Image Overlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            cueObject.transform.SetParent(transform, false);

            var rectTransform = cueObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = topOffset;
            rectTransform.sizeDelta = size;

            canvasGroup = cueObject.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            image = cueObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
        }

        private void Subscribe()
        {
            if (localization != null)
            {
                localization.LanguageChanged += HandleLanguageChanged;
            }

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
            {
                localization.LanguageChanged -= HandleLanguageChanged;
            }

            if (stageController != null)
            {
                stageController.StepChanged -= HandleStepChanged;
                stageController.StepSucceeded -= HandleStepSucceeded;
                stageController.StepFailed -= HandleStepFailed;
            }
        }

        private void HandleLanguageChanged()
        {
            if (showingFeedback)
            {
                return;
            }

            ShowInstructionByKey(currentInstructionKey);
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            ShowInstruction(step);
        }

        private void HandleStepSucceeded(SukiyakiStep step)
        {
            if (step == SukiyakiStep.Calibration || step == SukiyakiStep.Result)
            {
                return;
            }

            ShowFeedback(SuccessKey);
        }

        private void HandleStepFailed(SukiyakiStep step)
        {
            ShowFeedback(FailureKey);
        }

        private void ShowInstruction(SukiyakiStep step)
        {
            ShowInstructionByKey(KeyForStep(step));
        }

        private void ShowInstructionByKey(string key)
        {
            currentInstructionKey = key;
            if (showingFeedback)
            {
                pendingInstructionKey = key;
                return;
            }

            SetSprite(key);
        }

        private void ShowFeedback(string key)
        {
            showingFeedback = true;
            feedbackEndsAt = Time.unscaledTime + feedbackSeconds;
            SetSprite(key);
        }

        private void SetSprite(string key)
        {
            if (string.IsNullOrEmpty(key) || image == null)
            {
                return;
            }

            var resourcePath = ResourceRoot + LanguagePrefix() + "_" + key;
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                WarnMissingSprite(resourcePath);
                image.enabled = false;
                return;
            }

            image.enabled = true;
            image.sprite = sprite;
            canvasGroup.alpha = 1f;
            shownAt = Time.unscaledTime;
        }

        private string LanguagePrefix()
        {
            return localization != null && localization.CurrentLanguage == AppLanguage.English ? "en" : "ja";
        }

        private void WarnMissingSprite(string resourcePath)
        {
            if (warnedMissingSprites.Add(resourcePath))
            {
                Debug.LogWarning($"[Sukiyaki Cue] Missing cue sprite Resources/{resourcePath}.png", this);
            }
        }

        private static string KeyForStep(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.Calibration:
                    return "step_calibration";
                case SukiyakiStep.MixEgg:
                    return "step_mix_egg";
                case SukiyakiStep.PickMeat:
                    return "step_pick_meat";
                case SukiyakiStep.PutInBroth:
                    return "step_put_in_broth";
                case SukiyakiStep.FlipMeat:
                    return "step_flip_meat";
                case SukiyakiStep.DipEgg:
                    return "step_dip_egg";
                case SukiyakiStep.DeliverToMouth:
                    return "step_deliver";
                default:
                    return string.Empty;
            }
        }
    }
}
