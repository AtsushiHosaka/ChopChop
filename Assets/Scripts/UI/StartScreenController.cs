using ChopChop.App;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class StartScreenController : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/Start/start_background";

        private GameLocalization localization;
        private SceneNavigator navigator;
        private Text titleText;
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

            var main = new GameObject("Main", typeof(RectTransform));
            main.transform.SetParent(transform, false);
            var mainRect = main.GetComponent<RectTransform>();
            mainRect.anchorMin = new Vector2(0f, 0.5f);
            mainRect.anchorMax = new Vector2(0f, 0.5f);
            mainRect.pivot = new Vector2(0f, 0.5f);
            mainRect.anchoredPosition = new Vector2(128f, 0f);
            UiFactory.SetSize(mainRect, 620f, 560f);
            UiFactory.AddFadeSlideIn(main, 0.42f, 0.08f, new Vector2(-34f, 0f), 0.98f);

            UiFactory.AddVerticalLayout(main, 24f, TextAnchor.MiddleLeft, new RectOffset(0, 0, 0, 0));

            titleText = UiFactory.CreateText(main.transform, "Title", 88, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            UiFactory.SetSize(titleText.rectTransform, 620f, 130f);
            titleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var startButton = UiFactory.CreateButton(main.transform, "Start Button", string.Empty, new Color(0.95f, 0.32f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(startButton.GetComponent<RectTransform>(), 420f, 78f);
            startText = startButton.GetComponentInChildren<Text>();
            startButton.onClick.AddListener(navigator.LoadLevelSelect);

            var languageButton = UiFactory.CreateButton(main.transform, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 420f, 66f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var quitButton = UiFactory.CreateButton(main.transform, "Quit Button", string.Empty, new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(quitButton.GetComponent<RectTransform>(), 420f, 66f);
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
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.28f);
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
            titleText.text = localization.Text(LocalizationKeys.App.Title);
            startText.text = localization.Text(LocalizationKeys.App.Start);
            languageText.text = localization.Text(LocalizationKeys.App.Language);
            quitText.text = localization.Text(LocalizationKeys.App.Quit);
        }
    }
}
