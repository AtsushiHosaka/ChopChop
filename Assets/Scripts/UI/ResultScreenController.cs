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
        private Text totalLabelText;
        private Text totalValueText;
        private Text statsText;
        private StatRow timeRow;
        private StatRow speedRow;
        private StatRow careRow;
        private StatRow failureRow;
        private Text retryText;
        private Text levelSelectText;

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
            CreateReadableScrim();

            var panel = new GameObject("Result Panel", typeof(RectTransform));
            panel.transform.SetParent(transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, 18f);
            UiFactory.SetSize(panelRect, 1260f, 920f);
            UiFactory.AddFadeSlideIn(panel.gameObject, 0.38f, 0.04f, new Vector2(0f, -24f), 0.96f);

            var panelImage = UiFactory.CreateSpriteImage(panel.transform, "Panel Art", WaUiCatalog.ResultPanel, false);
            UiFactory.Stretch(panelImage.rectTransform, 0f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(panel.transform, false);
            UiFactory.Stretch(content.GetComponent<RectTransform>(), 0f);

            titleText = UiFactory.CreateText(content.transform, "Title Label", 64, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.34f, 0.18f, 0.09f, 1f));
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -46f);
            UiFactory.SetSize(titleRect, 340f, 82f);
            titleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.22f);

            var summary = new GameObject("Summary", typeof(RectTransform));
            summary.transform.SetParent(content.transform, false);
            var summaryRect = summary.GetComponent<RectTransform>();
            summaryRect.anchorMin = new Vector2(0.5f, 1f);
            summaryRect.anchorMax = new Vector2(0.5f, 1f);
            summaryRect.pivot = new Vector2(0.5f, 1f);
            summaryRect.anchoredPosition = new Vector2(0f, -146f);
            UiFactory.SetSize(summaryRect, 560f, 170f);

            totalLabelText = UiFactory.CreateText(summary.transform, "Total Label", 32, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.66f, 0.44f, 0.16f, 1f));
            var totalLabelRect = totalLabelText.rectTransform;
            totalLabelRect.anchorMin = new Vector2(0.5f, 1f);
            totalLabelRect.anchorMax = new Vector2(0.5f, 1f);
            totalLabelRect.pivot = new Vector2(0.5f, 1f);
            totalLabelRect.anchoredPosition = new Vector2(0f, 0f);
            UiFactory.SetSize(totalLabelRect, 340f, 46f);

            totalValueText = UiFactory.CreateText(summary.transform, "Total Value", 88, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.34f, 0.18f, 0.09f, 1f));
            var totalValueRect = totalValueText.rectTransform;
            totalValueRect.anchorMin = new Vector2(0.5f, 0f);
            totalValueRect.anchorMax = new Vector2(0.5f, 0f);
            totalValueRect.pivot = new Vector2(0.5f, 0f);
            totalValueRect.anchoredPosition = new Vector2(0f, 0f);
            UiFactory.SetSize(totalValueRect, 420f, 110f);
            totalValueText.gameObject.AddComponent<Shadow>().effectColor = new Color(1f, 0.97f, 0.9f, 0.42f);

            var statsArea = new GameObject("Stats Area", typeof(RectTransform));
            statsArea.transform.SetParent(content.transform, false);
            var statsAreaRect = statsArea.GetComponent<RectTransform>();
            statsAreaRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsAreaRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsAreaRect.pivot = new Vector2(0.5f, 0.5f);
            statsAreaRect.anchoredPosition = new Vector2(0f, -12f);
            UiFactory.SetSize(statsAreaRect, 760f, 360f);

            var bottomRule = UiFactory.CreatePanel(statsArea.transform, "Bottom Rule", new Color(0.9f, 0.74f, 0.37f, 0.48f));
            UiFactory.Anchor(bottomRule, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 0f), new Vector2(-22f, 2f));
            bottomRule.GetComponent<Image>().raycastTarget = false;

            timeRow = CreateStatRow(statsArea.transform, "Time", new Vector2(0f, 244f), LocalizationKeys.Hud.Time);
            speedRow = CreateStatRow(statsArea.transform, "Speed", new Vector2(0f, 176f), LocalizationKeys.Hud.Speed);
            careRow = CreateStatRow(statsArea.transform, "Care", new Vector2(0f, 108f), LocalizationKeys.Hud.Care);
            failureRow = CreateStatRow(statsArea.transform, "Failures", new Vector2(0f, 40f), LocalizationKeys.Hud.Failures);

            statsText = UiFactory.CreateText(content.transform, "Stats", 44, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.34f, 0.18f, 0.09f, 1f));
            var statsRect = statsText.rectTransform;
            statsRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsRect.pivot = new Vector2(0.5f, 0.5f);
            statsRect.anchoredPosition = new Vector2(0f, -24f);
            UiFactory.SetSize(statsRect, 900f, 340f);
            statsText.gameObject.AddComponent<Shadow>().effectColor = new Color(1f, 0.96f, 0.90f, 0.46f);

            var buttonRow = new GameObject("Button Row", typeof(RectTransform));
            buttonRow.transform.SetParent(content.transform, false);
            var buttonRowRect = buttonRow.GetComponent<RectTransform>();
            buttonRowRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRowRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRowRect.pivot = new Vector2(0.5f, 0f);
            buttonRowRect.anchoredPosition = new Vector2(0f, 122f);
            UiFactory.SetSize(buttonRowRect, 700f, 112f);
            UiFactory.AddHorizontalLayout(buttonRow, 34f, TextAnchor.MiddleCenter, new RectOffset(0, 0, 0, 0));

            var retryButton = UiFactory.CreateSpriteButton(
                buttonRow.transform,
                "Retry Button",
                WaUiCatalog.ButtonPrimary,
                WaUiCatalog.ButtonPrimaryHover,
                WaUiCatalog.ButtonPrimaryPressed);
            retryButton.transition = Selectable.Transition.None;
            var retryAnimator = retryButton.GetComponent<UiButtonAnimator>();
            if (retryAnimator != null)
            {
                DestroyComponent(retryAnimator);
            }
            UiFactory.SetSize(retryButton.GetComponent<RectTransform>(), 320f, 96f);
            retryText = UiFactory.CreateText(retryButton.transform, "Retry Label", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.1f, 0.05f, 1f));
            UiFactory.Stretch(retryText.rectTransform, 8f);
            retryButton.onClick.AddListener(() => navigator.LoadLevel(retrySceneName));

            var levelSelectButton = UiFactory.CreateSpriteButton(
                buttonRow.transform,
                "Level Select Button",
                WaUiCatalog.ButtonPrimary,
                WaUiCatalog.ButtonPrimaryHover,
                WaUiCatalog.ButtonPrimaryPressed);
            levelSelectButton.transition = Selectable.Transition.None;
            var levelSelectAnimator = levelSelectButton.GetComponent<UiButtonAnimator>();
            if (levelSelectAnimator != null)
            {
                DestroyComponent(levelSelectAnimator);
            }
            UiFactory.SetSize(levelSelectButton.GetComponent<RectTransform>(), 320f, 96f);
            levelSelectText = UiFactory.CreateText(levelSelectButton.transform, "Level Select Label", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.18f, 0.1f, 0.05f, 1f));
            UiFactory.Stretch(levelSelectText.rectTransform, 8f);
            levelSelectButton.onClick.AddListener(navigator.LoadStart);

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
            totalLabelText.text = localization.Text(LocalizationKeys.Result.Total);
            retryText.text = localization.Text(LocalizationKeys.Result.Retry);
            levelSelectText.text = localization.Text(LocalizationKeys.App.Back);

            if (hasVersusResult)
            {
                totalLabelText.gameObject.SetActive(false);
                totalValueText.gameObject.SetActive(false);
                SetStatRowsActive(false);
                statsText.gameObject.SetActive(true);
                statsText.text = VersusStatsText();
            }
            else
            {
                totalLabelText.gameObject.SetActive(true);
                totalValueText.gameObject.SetActive(true);
                SetStatRowsActive(true);
                statsText.gameObject.SetActive(false);
                totalValueText.text = result.TotalScore.ToString("0");
                ApplyStatRow(timeRow, result.CompletionTime.ToString("0.0") + "s");
                ApplyStatRow(speedRow, result.SpeedScore.ToString("0"));
                ApplyStatRow(careRow, result.CareScore.ToString("0"));
                ApplyStatRow(failureRow, result.FailureCount.ToString());
            }
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

        private void CreateReadableScrim()
        {
            var scrim = new GameObject("Readable Scrim", typeof(RectTransform), typeof(Image));
            scrim.transform.SetParent(transform, false);
            UiFactory.Stretch(scrim.GetComponent<RectTransform>(), 0f);
            var image = scrim.GetComponent<Image>();
            image.color = new Color(0.07f, 0.05f, 0.03f, 0.08f);
            image.raycastTarget = false;
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
            UiFactory.SetSize(rowRect, 660f, 60f);

            var label = UiFactory.CreateText(row.transform, "Label", 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.66f, 0.44f, 0.16f, 1f));
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, 0f);
            UiFactory.SetSize(labelRect, 190f, 42f);

            var value = UiFactory.CreateText(row.transform, "Value", 52, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.34f, 0.18f, 0.09f, 1f));
            var valueRect = value.rectTransform;
            valueRect.anchorMin = new Vector2(1f, 0.5f);
            valueRect.anchorMax = new Vector2(1f, 0.5f);
            valueRect.pivot = new Vector2(1f, 0.5f);
            valueRect.anchoredPosition = new Vector2(0f, 0f);
            UiFactory.SetSize(valueRect, 300f, 56f);

            return new StatRow(localizationKey, label, value, row);
        }

        private void ApplyStatRow(StatRow row, string value)
        {
            row.Label.text = localization.Text(row.LocalizationKey);
            row.Value.text = value;
        }

        private void SetStatRowsActive(bool isActive)
        {
            timeRow.Root.SetActive(isActive);
            speedRow.Root.SetActive(isActive);
            careRow.Root.SetActive(isActive);
            failureRow.Root.SetActive(isActive);
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(component);
                return;
            }

            DestroyImmediate(component);
        }

        private readonly struct StatRow
        {
            public readonly string LocalizationKey;
            public readonly Text Label;
            public readonly Text Value;
            public readonly GameObject Root;

            public StatRow(string localizationKey, Text label, Text value, GameObject root)
            {
                LocalizationKey = localizationKey;
                Label = label;
                Value = value;
                Root = root;
            }
        }
    }
}
