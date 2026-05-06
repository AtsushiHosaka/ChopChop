using System.Collections.Generic;
using ChopChop.App;
using ChopChop.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class LevelSelectScreenController : MonoBehaviour
    {
        private readonly List<LevelCard> cards = new List<LevelCard>();
        private GameLocalization localization;
        private SceneNavigator navigator;
        private Image titleImage;
        private Image backImage;
        private Image languageImage;

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
            CreateFrame();

            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(transform, false);
            var headerRect = header.GetComponent<RectTransform>();
            UiFactory.Anchor(headerRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-620f, -184f), new Vector2(620f, -28f));
            UiFactory.AddFadeSlideIn(header, 0.34f, 0.04f, new Vector2(0f, 18f), 0.99f);

            var titlePlaque = UiFactory.CreateSpriteImage(header.transform, "Title Plaque", WaUiCatalog.HeaderPlaque);
            titlePlaque.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            titlePlaque.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            titlePlaque.rectTransform.pivot = new Vector2(0f, 0.5f);
            titlePlaque.rectTransform.anchoredPosition = new Vector2(22f, 0f);
            UiFactory.SetSize(titlePlaque.rectTransform, 540f, 128f);

            titleImage = UiFactory.CreateSpriteImage(titlePlaque.transform, "Title Label", null, false);
            UiFactory.Anchor(titleImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(84f, 28f), new Vector2(-84f, -30f));

            var languageButton = UiFactory.CreateSpriteButton(
                header.transform,
                "Language Button",
                WaUiCatalog.ButtonSmall,
                WaUiCatalog.ButtonSmallHover,
                WaUiCatalog.ButtonSmallPressed);
            var languageRect = languageButton.GetComponent<RectTransform>();
            languageRect.anchorMin = new Vector2(1f, 0.5f);
            languageRect.anchorMax = new Vector2(1f, 0.5f);
            languageRect.pivot = new Vector2(1f, 0.5f);
            languageRect.anchoredPosition = new Vector2(-250f, 0f);
            UiFactory.SetSize(languageRect, 220f, 72f);
            languageImage = UiFactory.CreateSpriteImage(languageButton.transform, "Language Label", null, false);
            UiFactory.Anchor(languageImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(26f, 18f), new Vector2(-26f, -18f));
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var backButton = UiFactory.CreateSpriteButton(
                header.transform,
                "Back Button",
                WaUiCatalog.ButtonSmall,
                WaUiCatalog.ButtonSmallHover,
                WaUiCatalog.ButtonSmallPressed);
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(1f, 0.5f);
            backRect.anchorMax = new Vector2(1f, 0.5f);
            backRect.pivot = new Vector2(1f, 0.5f);
            backRect.anchoredPosition = new Vector2(-20f, 0f);
            UiFactory.SetSize(backRect, 220f, 72f);
            backImage = UiFactory.CreateSpriteImage(backButton.transform, "Back Label", null, false);
            UiFactory.Anchor(backImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(26f, 18f), new Vector2(-26f, -18f));
            backButton.onClick.AddListener(navigator.LoadStart);

            var gridObject = new GameObject("Level Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            UiFactory.Anchor(gridRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-560f, -248f), new Vector2(560f, 208f));

            var grid = gridObject.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(520f, 230f);
            grid.spacing = new Vector2(30f, 30f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;

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
            var button = UiFactory.CreateSpriteButton(
                parent,
                definition.levelId,
                WaUiCatalog.LevelCard,
                WaUiCatalog.LevelCard,
                WaUiCatalog.LevelCard);
            button.interactable = definition.isUnlocked;
            UiFactory.AddFadeSlideIn(button.gameObject, 0.34f, 0.08f + cards.Count * 0.045f, Vector2.zero, 1f);

            var thumbnail = CreateThumbnail(button.transform, ResolveThumbnail(definition), definition.isUnlocked);
            thumbnail.transform.SetAsFirstSibling();

            var overlay = UiFactory.CreatePanel(button.transform, "Thumbnail Tint", definition.isUnlocked
                ? new Color(0.29f, 0.14f, 0.06f, 0.20f)
                : new Color(0.18f, 0.10f, 0.08f, 0.50f));
            UiFactory.Stretch(overlay, 0f);
            overlay.transform.SetSiblingIndex(1);

            var label = UiFactory.CreateSpriteImage(button.transform, "Label", null, false);
            UiFactory.Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(22f, 116f), new Vector2(-22f, -22f));

            var description = UiFactory.CreateSpriteImage(button.transform, "Description", null, false);
            UiFactory.Anchor(description.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 54f), new Vector2(-24f, -94f));

            var state = UiFactory.CreateSpriteImage(button.transform, "State", null, false);
            UiFactory.Anchor(state.rectTransform, Vector2.zero, Vector2.one, new Vector2(320f, 154f), new Vector2(-30f, -36f));

            button.onClick.AddListener(() => navigator.LoadLevel(definition.sceneName));
            return new LevelCard(definition, label, description, state);
        }

        private RectTransform CreateThumbnail(Transform parent, Sprite sprite, bool isUnlocked)
        {
            var thumbnailObject = new GameObject("Thumbnail", typeof(RectTransform), typeof(Image));
            thumbnailObject.transform.SetParent(parent, false);
            var rectTransform = thumbnailObject.GetComponent<RectTransform>();
            UiFactory.Stretch(rectTransform, 0f);

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

            background.sprite = WaUiCatalog.LevelSelectBackground;
            background.color = Color.white;
            background.raycastTarget = false;
        }

        private void CreateFrame()
        {
            var frame = UiFactory.CreateSpriteImage(transform, "Wa Frame", WaUiCatalog.MainFrame, false);
            UiFactory.Anchor(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-640f, -380f), new Vector2(640f, 380f));
        }

        private void UpdateTexts()
        {
            UiFactory.ApplySprite(titleImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.LevelSelect));
            UiFactory.ApplySprite(languageImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.Language));
            UiFactory.ApplySprite(backImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.Back));

            foreach (var card in cards)
            {
                UiFactory.ApplySprite(card.Name, WaUiCatalog.LoadLocalizedKey(localization, card.Definition.displayNameKey));
                UiFactory.ApplySprite(card.Description, WaUiCatalog.LoadLocalizedKey(localization, card.Definition.descriptionKey));
                UiFactory.ApplySprite(card.State, card.Definition.isUnlocked
                    ? null
                    : WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.Level.Locked));
            }
        }

        private readonly struct LevelCard
        {
            public readonly LevelDefinition Definition;
            public readonly Image Name;
            public readonly Image Description;
            public readonly Image State;

            public LevelCard(LevelDefinition definition, Image name, Image description, Image state)
            {
                Definition = definition;
                Name = name;
                Description = description;
                State = state;
            }
        }
    }
}
