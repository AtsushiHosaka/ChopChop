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
        private Text languageText;

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

            var header = UiFactory.CreatePanel(transform, "Header", new Color(0.02f, 0.018f, 0.014f, 0.56f));
            UiFactory.Anchor(header, new Vector2(0f, 1f), new Vector2(0.58f, 1f), new Vector2(56f, -116f), new Vector2(-24f, -28f));
            UiFactory.AddFadeSlideIn(header.gameObject, 0.34f, 0.04f, new Vector2(0f, 18f), 0.99f);
            UiFactory.AddHorizontalLayout(header.gameObject, 20f, TextAnchor.MiddleLeft, new RectOffset(24, 24, 12, 12));

            titleText = UiFactory.CreateText(header, "Title", 44, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetSize(titleText.rectTransform, 560f, 66f);

            var languageButton = UiFactory.CreateButton(header, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 220f, 60f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var backButton = UiFactory.CreateButton(header, "Back Button", string.Empty, new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(backButton.GetComponent<RectTransform>(), 180f, 60f);
            backText = backButton.GetComponentInChildren<Text>();
            backButton.onClick.AddListener(navigator.LoadStart);

            var gridObject = new GameObject("Level List", typeof(RectTransform));
            gridObject.transform.SetParent(transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            UiFactory.Anchor(gridRect, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(80f, 84f), new Vector2(840f, -146f));
            UiFactory.AddFadeSlideIn(gridObject, 0.34f, 0.12f, new Vector2(-24f, 0f), 0.99f);

            UiFactory.AddVerticalLayout(gridObject, 14f, TextAnchor.UpperLeft, new RectOffset(0, 0, 0, 0));

            foreach (var level in catalog.Levels)
            {
                cards.Add(CreateCard(gridObject.transform, level));
            }

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
                ? new Color(0.08f, 0.065f, 0.052f, 0.88f)
                : new Color(0.08f, 0.08f, 0.08f, 0.66f);
            var button = UiFactory.CreateButton(parent, definition.levelId, string.Empty, color, Color.white);
            button.interactable = definition.isUnlocked;
            UiFactory.SetSize(button.GetComponent<RectTransform>(), 760f, 116f);
            UiFactory.AddFadeSlideIn(button.gameObject, 0.34f, 0.08f + cards.Count * 0.045f, Vector2.zero, 1f);

            var thumbnail = CreateThumbnail(button.transform, ResolveThumbnail(definition), definition.isUnlocked);
            thumbnail.transform.SetAsFirstSibling();

            var accent = UiFactory.CreatePanel(button.transform, "Accent", definition.isUnlocked
                ? new Color(0.95f, 0.36f, 0.22f, 1f)
                : new Color(0.45f, 0.38f, 0.28f, 0.8f));
            UiFactory.Anchor(accent, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(5f, 0f));

            var label = button.GetComponentInChildren<Text>();
            label.fontSize = 28;
            label.alignment = TextAnchor.MiddleLeft;
            UiFactory.Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(184f, 54f), new Vector2(-140f, -16f));
            label.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var description = UiFactory.CreateText(button.transform, "Description", 18, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(0.92f, 0.86f, 0.74f, 0.92f));
            UiFactory.Anchor(description.rectTransform, Vector2.zero, Vector2.one, new Vector2(184f, 16f), new Vector2(-110f, -58f));
            description.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.5f);

            var state = UiFactory.CreateText(button.transform, "State", 18, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.94f, 0.73f, 0.36f, 0.92f));
            UiFactory.Anchor(state.rectTransform, Vector2.zero, Vector2.one, new Vector2(560f, 0f), new Vector2(-24f, 0f));
            state.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            button.onClick.AddListener(() => navigator.LoadLevel(definition.sceneName));
            return new LevelCard(definition, label, description, state);
        }

        private RectTransform CreateThumbnail(Transform parent, Sprite sprite, bool isUnlocked)
        {
            var thumbnailObject = new GameObject("Thumbnail", typeof(RectTransform), typeof(Image));
            thumbnailObject.transform.SetParent(parent, false);
            var rectTransform = thumbnailObject.GetComponent<RectTransform>();
            UiFactory.Anchor(rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(5f, 0f), new Vector2(166f, 0f));

            var image = thumbnailObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = sprite == null
                ? new Color(0.18f, 0.18f, 0.18f, 1f)
                : (isUnlocked ? Color.white : new Color(0.48f, 0.48f, 0.48f, 0.72f));
            image.raycastTarget = false;
            image.preserveAspect = false;
            return rectTransform;
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
            rect.anchorMax = new Vector2(0.52f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = shade.GetComponent<Image>();
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.52f);
            image.raycastTarget = false;
        }

        private void UpdateTexts()
        {
            titleText.text = localization.Text(LocalizationKeys.App.LevelSelect);
            languageText.text = localization.Text(LocalizationKeys.App.Language);
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
