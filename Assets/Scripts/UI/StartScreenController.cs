using ChopChop.App;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class StartScreenController : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/Start/start_background_wafu";

        private GameLocalization localization;
        private SceneNavigator navigator;
        private Text titleText;
        private Text subtitleText;
        private Text languageText;
        private Text startText;
        private Text quitText;

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
            CreateReadableScrim();
            CreateLeftShade();

            var main = new GameObject("Main", typeof(RectTransform));
            main.transform.SetParent(transform, false);
            var mainRect = main.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0f, 0.5f);
            mainRect.anchorMax = new Vector2(0f, 0.5f);
            mainRect.pivot = new Vector2(0f, 0.5f);
            mainRect.anchoredPosition = new Vector2(112f, -8f);
            UiFactory.SetSize(mainRect, 620f, 620f);
            UiFactory.AddFadeSlideIn(main, 0.42f, 0.08f, new Vector2(-34f, 0f), 0.98f);

            UiFactory.AddVerticalLayout(main, 18f, TextAnchor.MiddleLeft, new RectOffset(0, 0, 0, 0));

            subtitleText = UiFactory.CreateText(main.transform, "Subtitle", 26, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.94f, 0.73f, 0.36f, 1f));
            UiFactory.SetSize(subtitleText.rectTransform, 480f, 38f);
            subtitleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            titleText = UiFactory.CreateText(main.transform, "Title", 88, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            titleText.alignment = TextAnchor.MiddleLeft;
            UiFactory.SetSize(titleText.rectTransform, 620f, 118f);
            titleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var rule = UiFactory.CreatePanel(main.transform, "Gold Rule", new Color(0.96f, 0.68f, 0.24f, 0.95f));
            UiFactory.SetSize(rule, 280f, 3f);

            var spacer = new GameObject("Button Spacer", typeof(RectTransform));
            spacer.transform.SetParent(main.transform, false);
            UiFactory.SetSize(spacer.GetComponent<RectTransform>(), 1f, 34f);

            var startButton = UiFactory.CreateButton(main.transform, "Start Button", string.Empty, new Color(0.95f, 0.32f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(startButton.GetComponent<RectTransform>(), 380f, 76f);
            startText = startButton.GetComponentInChildren<Text>();
            startButton.onClick.AddListener(navigator.LoadLevelSelect);

            var languageButton = UiFactory.CreateButton(main.transform, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 300f, 58f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var quitButton = UiFactory.CreateButton(main.transform, "Quit Button", string.Empty, new Color(0.12f, 0.12f, 0.12f, 0.88f), new Color(0.93f, 0.88f, 0.78f, 1f));
            UiFactory.SetSize(quitButton.GetComponent<RectTransform>(), 300f, 58f);
            quitText = quitButton.GetComponentInChildren<Text>();
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

            var sprite = Resources.Load<Sprite>(BackgroundResourcePath);
            if (sprite == null)
            {
                Debug.LogWarning($"[Start Screen] Missing background sprite Resources/{BackgroundResourcePath}.png", this);
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
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.18f);
            image.raycastTarget = false;
        }

        private void CreateLeftShade()
        {
            var shade = new GameObject("Left Shade", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(transform, false);
            var rect = shade.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0.54f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = shade.GetComponent<Image>();
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.42f);
            image.raycastTarget = false;
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
            subtitleText.text = localization.Text(LocalizationKeys.App.MenuSubtitle);
            titleText.text = localization.Text(LocalizationKeys.App.Title);
            startText.text = localization.Text(LocalizationKeys.App.Start);
            languageText.text = localization.Text(LocalizationKeys.App.Language);
            quitText.text = localization.Text(LocalizationKeys.App.Quit);
        }
    }
}
