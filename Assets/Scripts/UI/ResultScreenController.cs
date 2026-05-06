using ChopChop.App;
using ChopChop.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class ResultScreenController : MonoBehaviour
    {
        private static readonly Color CardFill = new Color(0.98f, 0.95f, 0.88f, 0.82f);
        private static readonly Color CardOutline = new Color(0.89f, 0.76f, 0.32f, 0.95f);
        private static readonly Color CardShadow = new Color(0.73f, 0.44f, 0.23f, 0.18f);
        private static readonly Color BodyTextColor = new Color(0.34f, 0.18f, 0.09f, 1f);
        private static readonly Color SecondaryTextColor = new Color(0.47f, 0.27f, 0.15f, 0.92f);

        private GameLocalization localization;
        private SceneNavigator navigator;
        private LevelRunResult result;
        private VersusLevelRunResult versusResult;
        private bool hasVersusResult;
        private string retrySceneName;

        private Image titleImage;
        private GameObject singlePlayerGroup;
        private GameObject versusGroup;
        private Text totalLabelText;
        private Text totalValueText;
        private StatTile timeTile;
        private StatTile speedTile;
        private StatTile careTile;
        private StatTile failureTile;
        private Text versusHeaderText;
        private VersusPlayerCard playerOneCard;
        private VersusPlayerCard playerTwoCard;
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
            panelRect.anchoredPosition = Vector2.zero;
            UiFactory.SetSize(panelRect, 920f, 660f);
            UiFactory.AddFadeSlideIn(panel, 0.38f, 0.04f, new Vector2(0f, -24f), 0.96f);

            var panelImage = UiFactory.CreateSpriteImage(panel.transform, "Panel Art", WaUiCatalog.ResultPanel, false);
            UiFactory.Stretch(panelImage.rectTransform, 0f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(panel.transform, false);
            UiFactory.Stretch(content.GetComponent<RectTransform>(), 28f);

            titleImage = UiFactory.CreateSpriteImage(content.transform, "Title Label", null, true);
            var titleRect = titleImage.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, 8f);
            UiFactory.SetSize(titleRect, 430f, 118f);

            singlePlayerGroup = new GameObject("Single Player Group", typeof(RectTransform));
            singlePlayerGroup.transform.SetParent(content.transform, false);
            var singleRect = singlePlayerGroup.GetComponent<RectTransform>();
            UiFactory.Stretch(singleRect, 0f);

            versusGroup = new GameObject("Versus Group", typeof(RectTransform));
            versusGroup.transform.SetParent(content.transform, false);
            var versusRect = versusGroup.GetComponent<RectTransform>();
            UiFactory.Stretch(versusRect, 0f);

            BuildSinglePlayerLayout(singlePlayerGroup.transform);
            BuildVersusLayout(versusGroup.transform);
            CreateButtons(content.transform);

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

        private void BuildSinglePlayerLayout(Transform parent)
        {
            var summaryCard = CreateCard(parent, "Summary Card", new Vector2(0.5f, 0.5f), new Vector2(0f, 122f), new Vector2(360f, 152f));
            totalLabelText = UiFactory.CreateText(summaryCard, "Total Label", 22, FontStyle.Bold, TextAnchor.MiddleCenter, SecondaryTextColor);
            var totalLabelRect = totalLabelText.rectTransform;
            totalLabelRect.anchorMin = new Vector2(0.5f, 1f);
            totalLabelRect.anchorMax = new Vector2(0.5f, 1f);
            totalLabelRect.pivot = new Vector2(0.5f, 1f);
            totalLabelRect.anchoredPosition = new Vector2(0f, -18f);
            UiFactory.SetSize(totalLabelRect, 250f, 28f);

            totalValueText = UiFactory.CreateText(summaryCard, "Total Value", 60, FontStyle.Bold, TextAnchor.MiddleCenter, BodyTextColor);
            var totalValueRect = totalValueText.rectTransform;
            totalValueRect.anchorMin = new Vector2(0.5f, 0.5f);
            totalValueRect.anchorMax = new Vector2(0.5f, 0.5f);
            totalValueRect.pivot = new Vector2(0.5f, 0.5f);
            totalValueRect.anchoredPosition = new Vector2(0f, -14f);
            UiFactory.SetSize(totalValueRect, 260f, 72f);

            timeTile = CreateStatTile(parent, "Time Tile", new Vector2(-142f, 8f), LocalizationKeys.Hud.Time);
            speedTile = CreateStatTile(parent, "Speed Tile", new Vector2(142f, 8f), LocalizationKeys.Hud.Speed);
            careTile = CreateStatTile(parent, "Care Tile", new Vector2(-142f, -112f), LocalizationKeys.Hud.Care);
            failureTile = CreateStatTile(parent, "Failure Tile", new Vector2(142f, -112f), LocalizationKeys.Hud.Failures);
        }

        private void BuildVersusLayout(Transform parent)
        {
            versusHeaderText = UiFactory.CreateText(parent, "Versus Header", 30, FontStyle.Bold, TextAnchor.MiddleCenter, BodyTextColor);
            var headerRect = versusHeaderText.rectTransform;
            headerRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerRect.anchorMax = new Vector2(0.5f, 0.5f);
            headerRect.pivot = new Vector2(0.5f, 0.5f);
            headerRect.anchoredPosition = new Vector2(0f, 168f);
            UiFactory.SetSize(headerRect, 620f, 40f);

            playerOneCard = CreateVersusPlayerCard(parent, "Player One Card", new Vector2(-182f, -8f));
            playerTwoCard = CreateVersusPlayerCard(parent, "Player Two Card", new Vector2(182f, -8f));
        }

        private void CreateButtons(Transform parent)
        {
            var retryButton = UiFactory.CreateSpriteButton(
                parent,
                "Retry Button",
                WaUiCatalog.ButtonPrimary,
                WaUiCatalog.ButtonPrimaryHover,
                WaUiCatalog.ButtonPrimaryPressed);
            var retryRect = retryButton.GetComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(0.5f, 0f);
            retryRect.anchorMax = new Vector2(0.5f, 0f);
            retryRect.pivot = new Vector2(0.5f, 0f);
            retryRect.anchoredPosition = new Vector2(-154f, 14f);
            UiFactory.SetSize(retryRect, 290f, 60f);
            retryImage = UiFactory.CreateSpriteImage(retryButton.transform, "Retry Label", null, false);
            UiFactory.Anchor(retryImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(30f, 14f), new Vector2(-30f, -12f));
            retryButton.onClick.AddListener(() => navigator.LoadLevel(retrySceneName));

            var levelSelectButton = UiFactory.CreateSpriteButton(
                parent,
                "Level Select Button",
                WaUiCatalog.ButtonSecondary,
                WaUiCatalog.ButtonSecondaryHover,
                WaUiCatalog.ButtonSecondaryPressed);
            var levelSelectRect = levelSelectButton.GetComponent<RectTransform>();
            levelSelectRect.anchorMin = new Vector2(0.5f, 0f);
            levelSelectRect.anchorMax = new Vector2(0.5f, 0f);
            levelSelectRect.pivot = new Vector2(0.5f, 0f);
            levelSelectRect.anchoredPosition = new Vector2(154f, 14f);
            UiFactory.SetSize(levelSelectRect, 290f, 60f);
            levelSelectImage = UiFactory.CreateSpriteImage(levelSelectButton.transform, "Level Select Label", null, false);
            UiFactory.Anchor(levelSelectImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(28f, 14f), new Vector2(-28f, -12f));
            levelSelectButton.onClick.AddListener(navigator.LoadLevelSelect);
        }

        private void UpdateTexts()
        {
            UiFactory.ApplySprite(titleImage, WaUiCatalog.LoadLabel(localization, "result_title"));
            UiFactory.ApplySprite(retryImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Result.Retry));
            UiFactory.ApplySprite(levelSelectImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Result.LevelSelect));

            if (hasVersusResult)
            {
                singlePlayerGroup.SetActive(false);
                versusGroup.SetActive(true);
                ApplyVersusTexts();
                return;
            }

            versusGroup.SetActive(false);
            singlePlayerGroup.SetActive(true);
            totalLabelText.text = localization.Text(LocalizationKeys.Result.Total);
            totalValueText.text = result.TotalScore.ToString("0");
            ApplyStatTile(timeTile, result.CompletionTime.ToString("0.0") + "s");
            ApplyStatTile(speedTile, result.SpeedScore.ToString("0"));
            ApplyStatTile(careTile, result.CareScore.ToString("0"));
            ApplyStatTile(failureTile, result.FailureCount.ToString());
        }

        private void ApplyVersusTexts()
        {
            var playerOneLabel = localization.Text(LocalizationKeys.Result.PlayerOne);
            var playerTwoLabel = localization.Text(LocalizationKeys.Result.PlayerTwo);
            versusHeaderText.text = versusResult.IsDraw
                ? localization.Text(LocalizationKeys.Result.Draw)
                : $"{localization.Text(LocalizationKeys.Result.Winner)}  {(versusResult.WinnerPlayerIndex == 0 ? playerOneLabel : playerTwoLabel)}";

            ApplyVersusPlayerCard(playerOneCard, playerOneLabel, versusResult.PlayerOne, !versusResult.IsDraw && versusResult.WinnerPlayerIndex == 0);
            ApplyVersusPlayerCard(playerTwoCard, playerTwoLabel, versusResult.PlayerTwo, !versusResult.IsDraw && versusResult.WinnerPlayerIndex == 1);
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

        private StatTile CreateStatTile(Transform parent, string name, Vector2 anchoredPosition, string localizationKey)
        {
            var root = CreateCard(parent, name, new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(248f, 104f));
            var label = UiFactory.CreateSpriteImage(root, "Label", null, false);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0.5f, 1f);
            labelRect.anchorMax = new Vector2(0.5f, 1f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.anchoredPosition = new Vector2(0f, -16f);
            UiFactory.SetSize(labelRect, LabelWidthFor(localizationKey), 24f);

            var value = UiFactory.CreateText(root, "Value", 40, FontStyle.Bold, TextAnchor.MiddleCenter, BodyTextColor);
            var valueRect = value.rectTransform;
            valueRect.anchorMin = new Vector2(0.5f, 0.5f);
            valueRect.anchorMax = new Vector2(0.5f, 0.5f);
            valueRect.pivot = new Vector2(0.5f, 0.5f);
            valueRect.anchoredPosition = new Vector2(0f, 10f);
            UiFactory.SetSize(valueRect, 190f, 52f);

            return new StatTile(localizationKey, root.gameObject, label, value);
        }

        private void ApplyStatTile(StatTile tile, string value)
        {
            UiFactory.ApplySprite(tile.Label, WaUiCatalog.LoadLocalizedKey(localization, tile.LocalizationKey));
            tile.Value.text = value;
        }

        private VersusPlayerCard CreateVersusPlayerCard(Transform parent, string name, Vector2 anchoredPosition)
        {
            var root = CreateCard(parent, name, new Vector2(0.5f, 0.5f), anchoredPosition, new Vector2(324f, 280f));

            var title = UiFactory.CreateText(root, "Player Label", 24, FontStyle.Bold, TextAnchor.MiddleCenter, SecondaryTextColor);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            UiFactory.SetSize(titleRect, 220f, 30f);

            var totalLabel = UiFactory.CreateText(root, "Total Label", 18, FontStyle.Bold, TextAnchor.MiddleCenter, SecondaryTextColor);
            var totalLabelRect = totalLabel.rectTransform;
            totalLabelRect.anchorMin = new Vector2(0.5f, 1f);
            totalLabelRect.anchorMax = new Vector2(0.5f, 1f);
            totalLabelRect.pivot = new Vector2(0.5f, 1f);
            totalLabelRect.anchoredPosition = new Vector2(0f, -58f);
            UiFactory.SetSize(totalLabelRect, 180f, 24f);

            var totalValue = UiFactory.CreateText(root, "Total Value", 52, FontStyle.Bold, TextAnchor.MiddleCenter, BodyTextColor);
            var totalValueRect = totalValue.rectTransform;
            totalValueRect.anchorMin = new Vector2(0.5f, 1f);
            totalValueRect.anchorMax = new Vector2(0.5f, 1f);
            totalValueRect.pivot = new Vector2(0.5f, 1f);
            totalValueRect.anchoredPosition = new Vector2(0f, -84f);
            UiFactory.SetSize(totalValueRect, 200f, 60f);

            var body = UiFactory.CreateText(root, "Body", 24, FontStyle.Bold, TextAnchor.UpperCenter, BodyTextColor);
            var bodyRect = body.rectTransform;
            bodyRect.anchorMin = new Vector2(0.5f, 0f);
            bodyRect.anchorMax = new Vector2(0.5f, 0f);
            bodyRect.pivot = new Vector2(0.5f, 0f);
            bodyRect.anchoredPosition = new Vector2(0f, 22f);
            UiFactory.SetSize(bodyRect, 240f, 108f);

            return new VersusPlayerCard(root.GetComponent<Image>(), title, totalLabel, totalValue, body);
        }

        private void ApplyVersusPlayerCard(VersusPlayerCard card, string playerLabel, LevelRunResult runResult, bool isWinner)
        {
            card.Background.color = isWinner ? new Color(1f, 0.97f, 0.86f, 0.9f) : CardFill;
            var outline = card.Background.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = isWinner ? new Color(0.92f, 0.73f, 0.24f, 1f) : CardOutline;
                outline.effectDistance = isWinner ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
            }

            card.Title.text = playerLabel;
            card.TotalLabel.text = localization.Text(LocalizationKeys.Result.Total);
            card.TotalValue.text = runResult.TotalScore.ToString("0");
            card.Body.text =
                $"{localization.Text(LocalizationKeys.Hud.Speed)}   {runResult.SpeedScore:0}\n" +
                $"{localization.Text(LocalizationKeys.Hud.Care)}   {runResult.CareScore:0}\n" +
                $"{localization.Text(LocalizationKeys.Hud.Failures)}   {runResult.FailureCount}";
        }

        private Transform CreateCard(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
        {
            var rect = UiFactory.CreatePanel(parent, name, CardFill);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            UiFactory.SetSize(rect, size.x, size.y);

            var image = rect.GetComponent<Image>();
            image.raycastTarget = false;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = CardOutline;
            outline.effectDistance = new Vector2(2f, -2f);

            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = CardShadow;
            shadow.effectDistance = new Vector2(0f, -10f);

            return rect;
        }

        private static float LabelWidthFor(string localizationKey)
        {
            switch (localizationKey)
            {
                case LocalizationKeys.Hud.Failures:
                    return 104f;
                case LocalizationKeys.Hud.Speed:
                    return 96f;
                case LocalizationKeys.Hud.Care:
                    return 92f;
                default:
                    return 100f;
            }
        }

        private readonly struct StatTile
        {
            public readonly string LocalizationKey;
            public readonly GameObject Root;
            public readonly Image Label;
            public readonly Text Value;

            public StatTile(string localizationKey, GameObject root, Image label, Text value)
            {
                LocalizationKey = localizationKey;
                Root = root;
                Label = label;
                Value = value;
            }
        }

        private readonly struct VersusPlayerCard
        {
            public readonly Image Background;
            public readonly Text Title;
            public readonly Text TotalLabel;
            public readonly Text TotalValue;
            public readonly Text Body;

            public VersusPlayerCard(Image background, Text title, Text totalLabel, Text totalValue, Text body)
            {
                Background = background;
                Title = title;
                TotalLabel = totalLabel;
                TotalValue = totalValue;
                Body = body;
            }
        }
    }
}
