using ChopChop.App;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class StartScreenController : MonoBehaviour
    {
        private GameLocalization localization;
        private SceneNavigator navigator;
        private Image titleImage;
        private Image languageImage;
        private Image startImage;
        private Image quitImage;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator)
        {
            var root = UiFactory.CreateScreen("Start Screen", new Color(0.05f, 0.07f, 0.08f, 1f), true);
            var controller = root.AddComponent<StartScreenController>();
            controller.Initialize(localization, navigator);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator)
        {
            localization = appLocalization;
            navigator = sceneNavigator;

            ApplyBackgroundImage();
            CreateFrame();

            var main = new GameObject("Main", typeof(RectTransform));
            main.transform.SetParent(transform, false);
            var mainRect = main.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0.5f, 0.5f);
            mainRect.anchorMax = new Vector2(0.5f, 0.5f);
            mainRect.pivot = new Vector2(0.5f, 0.5f);
            mainRect.anchoredPosition = new Vector2(0f, 34f);
            UiFactory.SetSize(mainRect, 760f, 620f);
            UiFactory.AddFadeSlideIn(main, 0.42f, 0.08f, new Vector2(0f, -24f), 0.98f);

            UiFactory.AddVerticalLayout(main, 18f, TextAnchor.UpperCenter, new RectOffset(0, 0, 0, 0));

            var header = UiFactory.CreateSpriteImage(main.transform, "Header", WaUiCatalog.HeaderPlaque);
            UiFactory.SetSize(header.rectTransform, 620f, 136f);

            titleImage = UiFactory.CreateSpriteImage(header.transform, "Title", null, false);
            UiFactory.Anchor(titleImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(92f, 24f), new Vector2(-92f, -32f));

            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(main.transform, false);
            UiFactory.SetSize(spacer.GetComponent<RectTransform>(), 10f, 12f);

            var startButton = UiFactory.CreateSpriteButton(
                main.transform,
                "Start Button",
                WaUiCatalog.ButtonPrimary,
                WaUiCatalog.ButtonPrimaryHover,
                WaUiCatalog.ButtonPrimaryPressed);
            UiFactory.SetSize(startButton.GetComponent<RectTransform>(), 430f, 96f);
            startImage = UiFactory.CreateSpriteImage(startButton.transform, "Start Label", null, false);
            UiFactory.Anchor(startImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(48f, 20f), new Vector2(-48f, -20f));
            startButton.onClick.AddListener(navigator.LoadLevelSelect);

            var languageButton = UiFactory.CreateSpriteButton(
                main.transform,
                "Language Button",
                WaUiCatalog.ButtonSecondary,
                WaUiCatalog.ButtonSecondaryHover,
                WaUiCatalog.ButtonSecondaryPressed);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 430f, 84f);
            languageImage = UiFactory.CreateSpriteImage(languageButton.transform, "Language Label", null, false);
            UiFactory.Anchor(languageImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(48f, 20f), new Vector2(-48f, -20f));
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var quitButton = UiFactory.CreateSpriteButton(
                main.transform,
                "Quit Button",
                WaUiCatalog.ButtonSecondary,
                WaUiCatalog.ButtonSecondaryHover,
                WaUiCatalog.ButtonSecondaryPressed);
            UiFactory.SetSize(quitButton.GetComponent<RectTransform>(), 430f, 84f);
            quitImage = UiFactory.CreateSpriteImage(quitButton.transform, "Quit Label", null, false);
            UiFactory.Anchor(quitImage.rectTransform, Vector2.zero, Vector2.one, new Vector2(48f, 20f), new Vector2(-48f, -20f));
            quitButton.onClick.AddListener(navigator.Quit);

            localization.LanguageChanged += UpdateTexts;
            UpdateTexts();
        }

        private void ApplyBackgroundImage()
        {
            var background = transform.Find("Background")?.GetComponent<Image>();
            if (background == null)
            {
                return;
            }

            background.sprite = WaUiCatalog.StartBackground;
            background.color = Color.white;
            background.raycastTarget = false;
        }

        private void CreateFrame()
        {
            var frame = UiFactory.CreateSpriteImage(transform, "Wa Frame", WaUiCatalog.MainFrame, false);
            UiFactory.Anchor(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-640f, -380f), new Vector2(640f, 380f));
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
            UiFactory.ApplySprite(titleImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.Title));
            UiFactory.ApplySprite(startImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.Start));
            UiFactory.ApplySprite(languageImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.Language));
            UiFactory.ApplySprite(quitImage, WaUiCatalog.LoadLocalizedKey(localization, LocalizationKeys.App.Quit));
        }
    }
}
