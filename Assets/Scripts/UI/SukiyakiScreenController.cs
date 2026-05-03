using ChopChop.App;
using ChopChop.Audio;
using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SukiyakiScreenController : MonoBehaviour
    {
        private GameLocalization localization;
        private SceneNavigator navigator;
        private SukiyakiStageController stage;
        private Text stepText;
        private Text timeText;
        private Text failureText;
        private Text languageText;
        private Text levelSelectText;
        private Slider progressBar;
        private SukiyakiCueOverlay cueOverlay;
        private UiPunchScale stepPunch;
        private UiPunchScale failurePunch;
        private int lastFailureCount = -1;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator)
        {
            var root = UiFactory.CreateScreen("Sukiyaki Screen", Color.clear, false);
            var controller = root.AddComponent<SukiyakiScreenController>();
            controller.Initialize(localization, navigator);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator)
        {
            localization = appLocalization;
            navigator = sceneNavigator;

            var references = FindAnyObjectByType<SukiyakiSceneReferences>();
            if (references == null || !references.IsReady)
            {
                CreateSetupMissingScreen();
                return;
            }

            stage = references.StageController;
            stage.Configure(references.HashiInputProvider, references.RailMover, references.MeatTearController);
            stage.StepChanged += HandleStepChanged;
            stage.Completed += HandleCompleted;

            CreateHud();
            CreateCueOverlay();
            localization.LanguageChanged += UpdateTexts;
            UpdateTexts();
            StartCountdown();
        }

        private void Update()
        {
            if (stage == null || timeText == null)
            {
                return;
            }

            timeText.text = $"{localization.Text(LocalizationKeys.Hud.Time)}: {stage.ElapsedSeconds:0.0}s";
            failureText.text = $"{localization.Text(LocalizationKeys.Hud.Failures)}: {stage.FailureCount}";
            if (lastFailureCount >= 0 && stage.FailureCount != lastFailureCount)
            {
                failurePunch?.Play();
            }

            lastFailureCount = stage.FailureCount;
            progressBar.value = stage.StepProgress01;
        }

        private void OnDestroy()
        {
            if (localization != null)
            {
                localization.LanguageChanged -= UpdateTexts;
            }

            if (stage != null)
            {
                stage.StepChanged -= HandleStepChanged;
                stage.Completed -= HandleCompleted;
            }
        }

        private void CreateHud()
        {
            var top = UiFactory.CreatePanel(transform, "HUD", new Color(0f, 0f, 0f, 0.48f));
            UiFactory.Anchor(top, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -110f), new Vector2(-24f, -20f));
            UiFactory.AddFadeSlideIn(top.gameObject, 0.28f, 0.04f, new Vector2(0f, 18f), 0.99f);
            UiFactory.AddHorizontalLayout(top.gameObject, 18f, TextAnchor.MiddleLeft, new RectOffset(20, 20, 10, 10));

            stepText = UiFactory.CreateText(top, "Step", 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetSize(stepText.rectTransform, 620f, 70f);
            stepPunch = stepText.gameObject.AddComponent<UiPunchScale>();

            timeText = UiFactory.CreateText(top, "Time", 27, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetSize(timeText.rectTransform, 220f, 70f);

            failureText = UiFactory.CreateText(top, "Failures", 27, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetSize(failureText.rectTransform, 180f, 70f);
            failurePunch = failureText.gameObject.AddComponent<UiPunchScale>();

            progressBar = UiFactory.CreateProgressBar(top, "Progress", new Color(1f, 1f, 1f, 0.22f), new Color(0.98f, 0.72f, 0.22f, 1f));
            UiFactory.SetSize(progressBar.GetComponent<RectTransform>(), 340f, 24f);

            var languageButton = UiFactory.CreateButton(top, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 170f, 58f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var levelSelectButton = UiFactory.CreateButton(top, "Level Select Button", string.Empty, new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(levelSelectButton.GetComponent<RectTransform>(), 190f, 58f);
            levelSelectText = levelSelectButton.GetComponentInChildren<Text>();
            levelSelectButton.onClick.AddListener(navigator.LoadLevelSelect);

        }

        private void CreateCueOverlay()
        {
            cueOverlay = gameObject.AddComponent<SukiyakiCueOverlay>();
            cueOverlay.Initialize(localization, stage);

            var audioBinder = gameObject.AddComponent<SukiyakiAudioBinder>();
            audioBinder.Initialize(stage);
        }

        private void StartCountdown()
        {
            var countdown = gameObject.AddComponent<SukiyakiCountdownOverlay>();
            countdown.Begin(() =>
            {
                if (stage == null)
                {
                    return;
                }

                stage.StartLevel();
                UpdateTexts();
            });
        }

        private void CreateSetupMissingScreen()
        {
            var panel = UiFactory.CreatePanel(transform, "Setup Missing", new Color(0.04f, 0.045f, 0.05f, 0.92f));
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.SetSize(panel, 680f, 260f);
            UiFactory.AddFadeSlideIn(panel.gameObject, 0.3f, 0f, new Vector2(0f, -18f), 0.96f);
            UiFactory.AddVerticalLayout(panel.gameObject, 24f, TextAnchor.MiddleCenter, new RectOffset(36, 36, 34, 34));

            var message = UiFactory.CreateText(panel, "Message", 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetSize(message.rectTransform, 600f, 92f);
            message.text = localization.Text(LocalizationKeys.Level.SetupMissing);

            var backButton = UiFactory.CreateButton(panel, "Level Select Button", localization.Text(LocalizationKeys.Result.LevelSelect), new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(backButton.GetComponent<RectTransform>(), 360f, 64f);
            backButton.onClick.AddListener(navigator.LoadLevelSelect);
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            UpdateTexts();
            stepPunch?.Play();
        }

        private void HandleCompleted(LevelRunResult result)
        {
            navigator.LoadResult(result, AppScenes.SukiyakiLevel);
        }

        private void UpdateTexts()
        {
            if (stage != null && stepText != null)
            {
                stepText.text = StepText(stage.CurrentStep);
            }

            languageText.text = localization.Text(LocalizationKeys.App.Language);
            levelSelectText.text = localization.Text(LocalizationKeys.Result.LevelSelect);
        }

        private string StepText(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.Calibration:
                    return localization.Text(LocalizationKeys.Hud.Calibrate);
                case SukiyakiStep.MixEgg:
                    return localization.Text(LocalizationKeys.Step.MixEgg);
                case SukiyakiStep.PickMeat:
                    return localization.Text(LocalizationKeys.Step.PickMeat);
                case SukiyakiStep.PutInBroth:
                    return localization.Text(LocalizationKeys.Step.PutInBroth);
                case SukiyakiStep.FlipMeat:
                    return localization.Text(LocalizationKeys.Step.FlipMeat);
                case SukiyakiStep.DipEgg:
                    return localization.Text(LocalizationKeys.Step.DipEgg);
                case SukiyakiStep.DeliverToMouth:
                    return localization.Text(LocalizationKeys.Step.Deliver);
                case SukiyakiStep.Result:
                    return localization.Text(LocalizationKeys.Result.Title);
                default:
                    return string.Empty;
            }
        }

    }
}
