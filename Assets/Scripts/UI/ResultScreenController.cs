using ChopChop.App;
using ChopChop.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class ResultScreenController : MonoBehaviour
    {
        private GameLocalization localization;
        private SceneNavigator navigator;
        private LevelRunResult result;
        private VersusLevelRunResult versusResult;
        private bool hasVersusResult;
        private string retrySceneName;
        private Text titleText;
        private Text statsText;
        private Text retryText;
        private Text levelSelectText;
        private Text languageText;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator, LevelRunResult result, string retrySceneName)
        {
            var root = UiFactory.CreateScreen("Result Screen", new Color(0.045f, 0.055f, 0.06f, 1f), true);
            var controller = root.AddComponent<ResultScreenController>();
            controller.Initialize(localization, navigator, result, retrySceneName);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator, LevelRunResult levelResult, string levelSceneName)
        {
            localization = appLocalization;
            navigator = sceneNavigator;
            result = levelResult;
            versusResult = LevelResultStore.CurrentVersusResult;
            hasVersusResult = LevelResultStore.HasVersusResult;
            retrySceneName = levelSceneName;

            var panel = UiFactory.CreatePanel(transform, "Result Panel", new Color(0.04f, 0.045f, 0.05f, 0.92f));
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.SetSize(panel, 700f, 520f);
            UiFactory.AddFadeSlideIn(panel.gameObject, 0.38f, 0.04f, new Vector2(0f, -24f), 0.96f);
            UiFactory.AddVerticalLayout(panel.gameObject, 22f, TextAnchor.MiddleCenter, new RectOffset(46, 46, 38, 38));

            titleText = UiFactory.CreateText(panel, "Title", 60, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetSize(titleText.rectTransform, 600f, 78f);

            statsText = UiFactory.CreateText(panel, "Stats", 32, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetSize(statsText.rectTransform, 600f, 170f);

            var retryButton = UiFactory.CreateButton(panel, "Retry Button", string.Empty, new Color(0.95f, 0.32f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(retryButton.GetComponent<RectTransform>(), 380f, 66f);
            retryText = retryButton.GetComponentInChildren<Text>();
            retryButton.onClick.AddListener(() => navigator.LoadLevel(retrySceneName));

            var levelSelectButton = UiFactory.CreateButton(panel, "Level Select Button", string.Empty, new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(levelSelectButton.GetComponent<RectTransform>(), 380f, 66f);
            levelSelectText = levelSelectButton.GetComponentInChildren<Text>();
            levelSelectButton.onClick.AddListener(navigator.LoadLevelSelect);

            var languageButton = UiFactory.CreateButton(transform, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            var languageRect = languageButton.GetComponent<RectTransform>();
            languageRect.anchorMin = new Vector2(1f, 1f);
            languageRect.anchorMax = new Vector2(1f, 1f);
            languageRect.pivot = new Vector2(1f, 1f);
            languageRect.anchoredPosition = new Vector2(-32f, -32f);
            UiFactory.SetSize(languageRect, 180f, 60f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            localization.LanguageChanged += UpdateTexts;
            UpdateTexts();
        }

        private void OnDestroy()
        {
            if (localization != null)
            {
                localization.LanguageChanged -= UpdateTexts;
            }
        }

        private void UpdateTexts()
        {
            titleText.text = localization.Text(LocalizationKeys.Result.Title);
            statsText.text = hasVersusResult ? VersusStatsText() : SinglePlayerStatsText(result);
            retryText.text = localization.Text(LocalizationKeys.Result.Retry);
            levelSelectText.text = localization.Text(LocalizationKeys.Result.LevelSelect);
            languageText.text = localization.Text(LocalizationKeys.App.Language);
        }

        private string SinglePlayerStatsText(LevelRunResult runResult)
        {
            return $"{localization.Text(LocalizationKeys.Hud.Time)}: {runResult.CompletionTime:0.0}s\n"
                + $"{localization.Text(LocalizationKeys.Hud.Speed)}: {runResult.SpeedScore:0}\n"
                + $"{localization.Text(LocalizationKeys.Hud.Care)}: {runResult.CareScore:0}\n"
                + $"{localization.Text(LocalizationKeys.Hud.Failures)}: {runResult.FailureCount}";
        }

        private string VersusStatsText()
        {
            var playerOneLabel = localization.Text(LocalizationKeys.Result.PlayerOne);
            var playerTwoLabel = localization.Text(LocalizationKeys.Result.PlayerTwo);
            var winnerText = versusResult.IsDraw
                ? localization.Text(LocalizationKeys.Result.Draw)
                : $"{localization.Text(LocalizationKeys.Result.Winner)}: {(versusResult.WinnerPlayerIndex == 0 ? playerOneLabel : playerTwoLabel)}";

            return $"{winnerText}\n"
                + PlayerLine(playerOneLabel, versusResult.PlayerOne)
                + "\n"
                + PlayerLine(playerTwoLabel, versusResult.PlayerTwo);
        }

        private string PlayerLine(string label, LevelRunResult runResult)
        {
            return $"{label}  {localization.Text(LocalizationKeys.Result.Total)}: {runResult.TotalScore:0}  "
                + $"{localization.Text(LocalizationKeys.Hud.Speed)}: {runResult.SpeedScore:0}  "
                + $"{localization.Text(LocalizationKeys.Hud.Care)}: {runResult.CareScore:0}  "
                + $"{localization.Text(LocalizationKeys.Hud.Failures)}: {runResult.FailureCount}";
        }
    }
}
