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
        private Image titleImage;
        private Text statsText;
        private StatRow timeRow;
        private StatRow speedRow;
        private StatRow careRow;
        private StatRow failureRow;
        private Image retryImage;
        private Image levelSelectImage;

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

            ApplyBackgroundImage();

            var panel = new GameObject("Result Panel", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.SetSize(panelRect, 760f, 548f);
            UiFactory.AddFadeSlideIn(panel.gameObject, 0.38f, 0.04f, new Vector2(0f, -24f), 0.96f);

            var panelImage = UiFactory.CreateSpriteImage(panel.transform, "Panel Art", WaUiCatalog.ResultPanel, false);
            UiFactory.Stretch(panelImage.rectTransform, 0f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(panel.transform, false);
            UiFactory.Stretch(content.GetComponent<RectTransform>(), 0f);

            titleImage = UiFactory.CreateSpriteImage(content.transform, "Title Label", null, true);
            var titleRect = titleImage.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -22f);
            UiFactory.SetSize(titleRect, 360f, 88f);

            timeRow = CreateStatRow(content.transform, "Time", new Vector2(0f, 262f), LocalizationKeys.Hud.Time);
            speedRow = CreateStatRow(content.transform, "Speed", new Vector2(0f, 218f), LocalizationKeys.Hud.Speed);
            careRow = CreateStatRow(content.transform, "Care", new Vector2(0f, 174f), LocalizationKeys.Hud.Care);
            failureRow = CreateStatRow(content.transform, "Failures", new Vector2(0f, 130f), LocalizationKeys.Hud.Failures);

            statsText = UiFactory.CreateText(content.transform, "Stats", 30, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.34f, 0.18f, 0.09f, 1f));
            var statsRect = statsText.rectTransform;
            statsRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsRect.pivot = new Vector2(0.5f, 0.5f);
            statsRect.anchoredPosition = new Vector2(0f, 116f);
            UiFactory.SetSize(statsRect, 560f, 180f);
            statsText.gameObject.AddComponent<Shadow>().effectColor = new Color(1f, 0.96f, 0.90f, 0.46f);

            var retryButton = UiFactory.CreateSpriteButton(
                content.transform,
                "Retry Button",
                WaUiCatalog.ButtonPrimary,
                WaUiCatalog.ButtonPrimaryHover,
                WaUiCatalog.ButtonPrimaryPressed);
            var retryRect = retryButton.GetComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(0.5f, 0f);
            retryRect.anchorMax = new Vector2(0.5f, 0f);
            retryRect.pivot = new Vector2(0.5f, 0f);
            retryRect.anchoredPosition = new Vector2(0f, 66f);
            UiFactory.SetSize(retryRect, 360f, 56f);
            retryImage = UiFactory.CreateSpriteImage(retryButton.transform, "Retry Label", null, false);
            UiFactory.Anchor(retryImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 12f), new Vector2(-28f, -10f));
            retryButton.onClick.AddListener(() => navigator.LoadLevel(retrySceneName));

            var levelSelectButton = UiFactory.CreateSpriteButton(
                content.transform,
                "Level Select Button",
                WaUiCatalog.ButtonSecondary,
                WaUiCatalog.ButtonSecondaryHover,
                WaUiCatalog.ButtonSecondaryPressed);
            var levelSelectRect = levelSelectButton.GetComponent<RectTransform>();
            levelSelectRect.anchorMin = new Vector2(0.5f, 0f);
            levelSelectRect.anchorMax = new Vector2(0.5f, 0f);
            levelSelectRect.pivot = new Vector2(0.5f, 0f);
            levelSelectRect.anchoredPosition = new Vector2(0f, 18f);
            UiFactory.SetSize(levelSelectRect, 360f, 52f);
            levelSelectImage = UiFactory.CreateSpriteImage(levelSelectButton.transform, "Level Select Label", null, false);
            UiFactory.Anchor(levelSelectImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 10f), new Vector2(-28f, -8f));
            levelSelectButton.onClick.AddListener(navigator.LoadLevelSelect);

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
            UiFactory.ApplySprite(titleImage, WaUiCatalog.LoadLabel(localization, "result_title"));
            if (hasVersusResult)
            {
                SetStatRowsActive(false);
                statsText.gameObject.SetActive(true);
                statsText.text = VersusStatsText();
            }
            else
            {
                SetStatRowsActive(true);
                statsText.gameObject.SetActive(false);
                ApplyStatRow(timeRow, result.CompletionTime.ToString("0.0") + "s");
                ApplyStatRow(speedRow, result.SpeedScore.ToString("0"));
                ApplyStatRow(careRow, result.CareScore.ToString("0"));
                ApplyStatRow(failureRow, result.FailureCount.ToString());
            }

            UiFactory.ApplySprite(retryImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Result.Retry));
            UiFactory.ApplySprite(levelSelectImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Result.LevelSelect));
        }

        private void ApplyBackgroundImage()
        {
            var background = transform.Find("Background")?.GetComponent<Image>();
            if (background == null)
            {
                return;
            }

            background.sprite = WaUiCatalog.ResultBackground;
            background.color = Color.white;
            background.raycastTarget = false;
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

        private StatRow CreateStatRow(Transform parent, string name, Vector2 anchoredPosition, string localizationKey)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0f);
            rowRect.anchorMax = new Vector2(0.5f, 0f);
            rowRect.pivot = new Vector2(0.5f, 0f);
            rowRect.anchoredPosition = anchoredPosition;
            UiFactory.SetSize(rowRect, 280f, 32f);

            var label = UiFactory.CreateSpriteImage(row.transform, "Label", null, false);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, 0f);
            UiFactory.SetSize(labelRect, 78f, 22f);

            var value = UiFactory.CreateText(row.transform, "Value", 30, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.34f, 0.18f, 0.09f, 1f));
            var valueRect = value.rectTransform;
            valueRect.anchorMin = new Vector2(1f, 0.5f);
            valueRect.anchorMax = new Vector2(1f, 0.5f);
            valueRect.pivot = new Vector2(1f, 0.5f);
            valueRect.anchoredPosition = new Vector2(0f, 0f);
            UiFactory.SetSize(valueRect, 136f, 26f);

            return new StatRow(localizationKey, label, value, row);
        }

        private void ApplyStatRow(StatRow row, string value)
        {
            UiFactory.ApplySprite(row.Label, WaUiCatalog.LoadLocalizedKey(localization, row.LocalizationKey));
            row.Value.text = value;
        }

        private void SetStatRowsActive(bool isActive)
        {
            timeRow.Root.SetActive(isActive);
            speedRow.Root.SetActive(isActive);
            careRow.Root.SetActive(isActive);
            failureRow.Root.SetActive(isActive);
        }

        private readonly struct StatRow
        {
            public readonly string LocalizationKey;
            public readonly Image Label;
            public readonly Text Value;
            public readonly GameObject Root;

            public StatRow(string localizationKey, Image label, Text value, GameObject root)
            {
                LocalizationKey = localizationKey;
                Label = label;
                Value = value;
                Root = root;
            }
        }
    }
}
