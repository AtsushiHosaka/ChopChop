using System.Collections.Generic;
using ChopChop.App;
using ChopChop.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class LevelSelectScreenController : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/LevelSelect/level_select_background_wafu";

        private readonly List<LevelCard> cards = new List<LevelCard>();
        private GameLocalization localization;
        private SceneNavigator navigator;
        private Text titleText;
        private Text backText;
        private Text settingsText;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator, LevelCatalog catalog)
        {
            var root = UiFactory.CreateScreen("Level Select Screen", new Color(0.06f, 0.07f, 0.08f, 1f), true);
            var controller = root.AddComponent<LevelSelectScreenController>();
            controller.Initialize(localization, navigator, catalog);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator, LevelCatalog catalog)
        {
            localization = appLocalization;
            navigator = sceneNavigator;

            ApplyBackgroundImage();
            CreateReadableScrim();
            CreateLeftShade();

            var menuPanel = UiFactory.CreatePanel(transform, "Level Menu Panel", new Color(0.025f, 0.02f, 0.015f, 0.64f));
            UiFactory.Anchor(menuPanel, new Vector2(0.035f, 0.06f), new Vector2(0.72f, 0.94f), Vector2.zero, Vector2.zero);
            UiFactory.AddFadeSlideIn(menuPanel.gameObject, 0.34f, 0.04f, new Vector2(-20f, 0f), 0.99f);

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(menuPanel, false);
            UiFactory.Anchor(header.GetComponent<RectTransform>(), new Vector2(0f, 0.78f), new Vector2(1f, 0.98f), new Vector2(22f, 0f), new Vector2(-22f, 0f));
            UiFactory.AddHorizontalLayout(header, 16f, TextAnchor.MiddleLeft, new RectOffset(0, 0, 0, 0));

            titleText = UiFactory.CreateText(header.transform, "Title", 44, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetFlexibleFill(titleText.rectTransform);
            titleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var settingsButton = UiFactory.CreateButton(header.transform, "Settings Button", string.Empty, new Color(0.34f, 0.23f, 0.12f, 0.96f), new Color(0.98f, 0.92f, 0.82f, 1f));
            UiFactory.SetSize(settingsButton.GetComponent<RectTransform>(), 164f, 56f);
            settingsText = settingsButton.GetComponentInChildren<Text>();
            settingsButton.onClick.AddListener(navigator.LoadSettings);

            var backButton = UiFactory.CreateButton(header.transform, "Back Button", string.Empty, new Color(0.11f, 0.105f, 0.095f, 0.94f), new Color(0.95f, 0.89f, 0.78f, 1f));
            UiFactory.SetSize(backButton.GetComponent<RectTransform>(), 144f, 56f);
            backText = backButton.GetComponentInChildren<Text>();
            backButton.onClick.AddListener(navigator.LoadStart);

            var rule = UiFactory.CreatePanel(menuPanel, "Gold Rule", new Color(0.93f, 0.63f, 0.22f, 0.92f));
            UiFactory.Anchor(rule, new Vector2(0f, 0.735f), new Vector2(1f, 0.735f), new Vector2(22f, 0f), new Vector2(-22f, 3f));

            var viewportObject = new GameObject("Level Viewport", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            viewportObject.transform.SetParent(menuPanel, false);
            var viewportRect = viewportObject.GetComponent<RectTransform>();
            UiFactory.Anchor(viewportRect, new Vector2(0f, 0.055f), new Vector2(1f, 0.705f), new Vector2(22f, 0f), new Vector2(-46f, 0f));
            var viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportImage.raycastTarget = true;
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            var scrollbar = CreateScrollbar(menuPanel);

            var gridObject = new GameObject("Level Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(UiResponsiveGridLayout));
            gridObject.transform.SetParent(viewportObject.transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            UiFactory.Anchor(gridRect, new Vector2(0f, 1f), Vector2.one, Vector2.zero, Vector2.zero);
            gridRect.pivot = new Vector2(0.5f, 1f);

            gridObject.GetComponent<UiResponsiveGridLayout>().Configure(
                2,
                360f,
                2.35f,
                new Vector2(18f, 18f),
                new RectOffset(0, 0, 0, 0));

            var scrollRect = viewportObject.GetComponent<ScrollRect>();
            scrollRect.content = gridRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 34f;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.verticalScrollbarSpacing = 10f;

            foreach (var level in catalog.Levels)
            {
                cards.Add(CreateCard(gridObject.transform, level));
            }

            scrollRect.verticalNormalizedPosition = 1f;

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

        private LevelCard CreateCard(Transform parent, LevelDefinition definition)
        {
            var color = definition.isUnlocked
                ? new Color(0.095f, 0.068f, 0.044f, 0.9f)
                : new Color(0.075f, 0.07f, 0.065f, 0.68f);
            var button = UiFactory.CreateButton(parent, definition.levelId, string.Empty, color, Color.white);
            button.interactable = definition.isUnlocked;
            UiFactory.AddFadeSlideIn(button.gameObject, 0.34f, 0.08f + cards.Count * 0.045f, Vector2.zero, 1f);

            var thumbnail = CreateThumbnail(button.transform, ResolveThumbnail(definition), definition.isUnlocked);
            thumbnail.transform.SetAsFirstSibling();

            var textBand = UiFactory.CreatePanel(button.transform, "Text Band", new Color(0.02f, 0.015f, 0.012f, 0.74f));
            UiFactory.Anchor(textBand, Vector2.zero, new Vector2(1f, 0.32f), Vector2.zero, Vector2.zero);
            textBand.GetComponent<Image>().raycastTarget = false;

            var accent = UiFactory.CreatePanel(button.transform, "Accent", definition.isUnlocked
                ? new Color(0.95f, 0.36f, 0.22f, 1f)
                : new Color(0.45f, 0.38f, 0.28f, 0.8f));
            UiFactory.Anchor(accent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(14f, 0f), new Vector2(-14f, 4f));
            accent.GetComponent<Image>().raycastTarget = false;

            var label = button.GetComponentInChildren<Text>();
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleLeft;
            UiFactory.Anchor(label.rectTransform, new Vector2(0f, 0.13f), new Vector2(1f, 0.31f), new Vector2(18f, 0f), new Vector2(-18f, 0f));
            label.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);
            label.transform.SetAsLastSibling();

            var description = UiFactory.CreateText(button.transform, "Description", 16, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.92f, 0.86f, 0.74f, 0.92f));
            UiFactory.Anchor(description.rectTransform, new Vector2(0f, 0.035f), new Vector2(1f, 0.15f), new Vector2(18f, 0f), new Vector2(-18f, 0f));
            description.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.5f);
            description.transform.SetAsLastSibling();

            var state = UiFactory.CreateText(button.transform, "State", 18, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.94f, 0.73f, 0.36f, 0.92f));
            UiFactory.Anchor(state.rectTransform, new Vector2(0.46f, 0.78f), Vector2.one, new Vector2(0f, 0f), new Vector2(-18f, -12f));
            state.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);
            state.transform.SetAsLastSibling();

            button.onClick.AddListener(() => navigator.LoadLevel(definition.sceneName));
            return new LevelCard(definition, label, description, state);
        }

        private RectTransform CreateThumbnail(Transform parent, Sprite sprite, bool isUnlocked)
        {
            var frame = UiFactory.CreatePanel(parent, "Thumbnail Frame", new Color(0.035f, 0.026f, 0.018f, 0.92f));
            UiFactory.Anchor(frame, new Vector2(0f, 0.32f), Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            frame.GetComponent<Image>().raycastTarget = false;

            var thumbnailObject = new GameObject("Thumbnail", typeof(RectTransform), typeof(Image));
            thumbnailObject.transform.SetParent(frame, false);
            var rectTransform = thumbnailObject.GetComponent<RectTransform>();
            UiFactory.Stretch(rectTransform, 0f);

            var image = thumbnailObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = sprite == null
                ? new Color(0.18f, 0.18f, 0.18f, 1f)
                : (isUnlocked ? Color.white : new Color(0.48f, 0.48f, 0.48f, 0.72f));
            image.raycastTarget = false;
            image.preserveAspect = true;
            return frame;
        }

        private Scrollbar CreateScrollbar(Transform parent)
        {
            var scrollbarObject = new GameObject("Level Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObject.transform.SetParent(parent, false);
            var scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
            UiFactory.Anchor(scrollbarRect, new Vector2(1f, 0.055f), new Vector2(1f, 0.705f), new Vector2(-32f, 0f), new Vector2(-18f, 0f));

            var trackImage = scrollbarObject.GetComponent<Image>();
            trackImage.color = new Color(0.035f, 0.026f, 0.018f, 0.86f);

            var slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarObject.transform, false);
            UiFactory.Stretch(slidingArea.GetComponent<RectTransform>(), 2f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(slidingArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            UiFactory.Stretch(handleRect, 0f);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(0.94f, 0.64f, 0.22f, 0.95f);

            var scrollbar = scrollbarObject.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;
            scrollbar.size = 0.55f;
            return scrollbar;
        }

        private static Sprite ResolveThumbnail(LevelDefinition definition)
        {
            if (definition.thumbnail != null)
            {
                return definition.thumbnail;
            }

            switch (definition.sceneName)
            {
                case AppScenes.TutorialTempura:
                    return Resources.Load<Sprite>("UI/LevelThumbnails/tutorial_tempura");
                case AppScenes.Level01Sashimi:
                    return Resources.Load<Sprite>("UI/LevelThumbnails/level01_sashimi");
                case AppScenes.Level02TamagoKakeGohan:
                    return Resources.Load<Sprite>("UI/LevelThumbnails/level02_tamago_kake_gohan");
                case AppScenes.Level03Tofu:
                    return Resources.Load<Sprite>("UI/LevelThumbnails/level03_tofu");
                case AppScenes.Level04GrilledFish:
                    return Resources.Load<Sprite>("UI/LevelThumbnails/level04_grilled_fish");
                case AppScenes.SukiyakiLevel:
                    return Resources.Load<Sprite>("UI/LevelThumbnails/level05_sukiyaki");
                default:
                    return null;
            }
        }

        private void ApplyBackgroundImage()
        {
            var background = transform.Find("Background")?.GetComponent<Image>();
            if (background == null)
            {
                return;
            }

            var sprite = Resources.Load<Sprite>(BackgroundResourcePath);
            if (sprite == null)
            {
                Debug.LogWarning($"[Level Select] Missing background sprite Resources/{BackgroundResourcePath}.png", this);
                return;
            }

            background.sprite = sprite;
            background.color = Color.white;
            background.raycastTarget = false;
        }

        private void CreateReadableScrim()
        {
            var scrim = new GameObject("Readable Scrim", typeof(RectTransform), typeof(Image));
            scrim.transform.SetParent(transform, false);
            UiFactory.Stretch(scrim.GetComponent<RectTransform>(), 0f);
            var image = scrim.GetComponent<Image>();
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.24f);
            image.raycastTarget = false;
        }

        private void CreateLeftShade()
        {
            var shade = new GameObject("Left Shade", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(transform, false);
            var rect = shade.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0.74f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = shade.GetComponent<Image>();
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.52f);
            image.raycastTarget = false;
        }

        private void UpdateTexts()
        {
            titleText.text = localization.Text(LocalizationKeys.App.LevelSelect);
            settingsText.text = localization.Text(LocalizationKeys.App.Settings);
            backText.text = localization.Text(LocalizationKeys.App.Back);

            foreach (var card in cards)
            {
                card.Name.text = localization.Text(card.Definition.displayNameKey);
                card.Description.text = localization.Text(card.Definition.descriptionKey);
                card.State.text = card.Definition.isUnlocked ? string.Empty : localization.Text(LocalizationKeys.Level.Locked);
            }
        }

        private readonly struct LevelCard
        {
            public readonly LevelDefinition Definition;
            public readonly Text Name;
            public readonly Text Description;
            public readonly Text State;

            public LevelCard(LevelDefinition definition, Text name, Text description, Text state)
            {
                Definition = definition;
                Name = name;
                Description = description;
                State = state;
            }
        }
    }
}
