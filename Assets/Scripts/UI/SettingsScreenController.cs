using ChopChop.App;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class SettingsScreenController : MonoBehaviour
    {
        private const string BackgroundResourcePath = "UI/Start/start_background_wafu";

        private static readonly (AppLanguage lang, string nativeLabel)[] LanguageOptions =
        {
            (AppLanguage.Japanese,          "日本語"),
            (AppLanguage.English,           "English"),
            (AppLanguage.Korean,            "한국어"),
            (AppLanguage.ChineseSimplified, "中文(简体)"),
            (AppLanguage.French,            "Français"),
            (AppLanguage.Spanish,           "Español"),
        };

        private static readonly Color SelectedColor   = new Color(0.72f, 0.18f, 0.11f, 0.92f);
        private static readonly Color UnselectedColor = new Color(0.20f, 0.17f, 0.13f, 0.90f);

        private GameLocalization localization;
        private SceneNavigator navigator;
        private RectTransform settingsPanel;
        private RectTransform languagePanel;
        private Text settingsTitleText;
        private Text languageButtonText;
        private Text settingsBackText;
        private Text languageTitleText;
        private Text languageBackText;
        private Button[] languageOptionButtons;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator)
        {
            var root = UiFactory.CreateScreen("Settings Screen", new Color(0.045f, 0.05f, 0.05f, 1f), true);
            var controller = root.AddComponent<SettingsScreenController>();
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
            CreateSettingsPanel();
            CreateLanguagePanel();
            ShowSettingsPanel();

            localization.LanguageChanged += UpdateTexts;
            UpdateTexts();
        }

        private void CreateSettingsPanel()
        {
            settingsPanel = UiFactory.CreatePanel(transform, "Settings Panel", new Color(0.025f, 0.02f, 0.015f, 0.72f));
            UiFactory.Anchor(settingsPanel, new Vector2(0.09f, 0.08f), new Vector2(0.48f, 0.92f), Vector2.zero, Vector2.zero);
            UiFactory.AddFadeSlideIn(settingsPanel.gameObject, 0.38f, 0.04f, new Vector2(-20f, 0f), 0.98f);
            UiFactory.AddVerticalLayout(settingsPanel.gameObject, 16f, TextAnchor.UpperLeft, new RectOffset(34, 34, 30, 30));

            settingsTitleText = UiFactory.CreateText(settingsPanel, "Title", 52, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetPreferredHeight(settingsTitleText.rectTransform, 68f);
            settingsTitleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var rule = UiFactory.CreatePanel(settingsPanel, "Gold Rule", new Color(0.93f, 0.63f, 0.22f, 0.92f));
            UiFactory.SetPreferredHeight(rule, 2f);

            var languageButton = UiFactory.CreateButton(settingsPanel, "Language Settings Button", string.Empty, UnselectedColor, new Color(0.98f, 0.92f, 0.82f, 1f));
            UiFactory.SetPreferredHeight(languageButton.GetComponent<RectTransform>(), 58f);
            languageButtonText = languageButton.GetComponentInChildren<Text>();
            languageButtonText.alignment = TextAnchor.MiddleLeft;
            UiFactory.Anchor(languageButtonText.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 0f), new Vector2(-16f, 0f));
            languageButton.onClick.AddListener(ShowLanguagePanel);

            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(settingsPanel, false);
            UiFactory.SetFlexibleFill(spacer.GetComponent<RectTransform>());

            var backButton = UiFactory.CreateButton(settingsPanel, "Back Button", string.Empty, new Color(0.11f, 0.105f, 0.095f, 0.94f), new Color(0.95f, 0.89f, 0.78f, 1f));
            UiFactory.SetPreferredHeight(backButton.GetComponent<RectTransform>(), 56f);
            settingsBackText = backButton.GetComponentInChildren<Text>();
            backButton.onClick.AddListener(navigator.LoadSettingsReturn);
        }

        private void CreateLanguagePanel()
        {
            languagePanel = UiFactory.CreatePanel(transform, "Language Settings Panel", new Color(0.025f, 0.02f, 0.015f, 0.72f));
            UiFactory.Anchor(languagePanel, new Vector2(0.09f, 0.08f), new Vector2(0.48f, 0.92f), Vector2.zero, Vector2.zero);
            UiFactory.AddVerticalLayout(languagePanel.gameObject, 14f, TextAnchor.UpperLeft, new RectOffset(34, 34, 30, 30));

            languageTitleText = UiFactory.CreateText(languagePanel, "Title", 52, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetPreferredHeight(languageTitleText.rectTransform, 68f);
            languageTitleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var rule = UiFactory.CreatePanel(languagePanel, "Gold Rule", new Color(0.93f, 0.63f, 0.22f, 0.92f));
            UiFactory.SetPreferredHeight(rule, 2f);

            languageOptionButtons = new Button[LanguageOptions.Length];
            for (var i = 0; i < LanguageOptions.Length; i++)
            {
                var (lang, nativeLabel) = LanguageOptions[i];
                var btn = UiFactory.CreateButton(languagePanel, $"Lang_{lang}", nativeLabel, UnselectedColor, new Color(0.98f, 0.92f, 0.82f, 1f));
                UiFactory.SetPreferredHeight(btn.GetComponent<RectTransform>(), 52f);
                var btnText = btn.GetComponentInChildren<Text>();
                btnText.alignment = TextAnchor.MiddleLeft;
                UiFactory.Anchor(btnText.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 0f), new Vector2(-16f, 0f));
                var capturedLang = lang;
                btn.onClick.AddListener(() => localization.SetLanguage(capturedLang));
                languageOptionButtons[i] = btn;
            }

            var secondRule = UiFactory.CreatePanel(languagePanel, "Second Rule", new Color(0.93f, 0.63f, 0.22f, 0.40f));
            UiFactory.SetPreferredHeight(secondRule, 1f);

            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(languagePanel, false);
            UiFactory.SetFlexibleFill(spacer.GetComponent<RectTransform>());

            var backButton = UiFactory.CreateButton(languagePanel, "Language Back Button", string.Empty, new Color(0.11f, 0.105f, 0.095f, 0.94f), new Color(0.95f, 0.89f, 0.78f, 1f));
            UiFactory.SetPreferredHeight(backButton.GetComponent<RectTransform>(), 56f);
            languageBackText = backButton.GetComponentInChildren<Text>();
            backButton.onClick.AddListener(ShowSettingsPanel);
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
                Debug.LogWarning($"[Settings Screen] Missing background sprite Resources/{BackgroundResourcePath}.png", this);
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

        private void CreateLeftShade()
        {
            var shade = new GameObject("Left Shade", typeof(RectTransform), typeof(Image));
            shade.transform.SetParent(transform, false);
            var rect = shade.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0.58f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = shade.GetComponent<Image>();
            image.color = new Color(0.02f, 0.018f, 0.014f, 0.48f);
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
            settingsTitleText.text = localization.Text(LocalizationKeys.Settings.Title);
            languageButtonText.text = localization.Text(LocalizationKeys.Settings.Language);
            settingsBackText.text = localization.Text(LocalizationKeys.App.Back);
            languageTitleText.text = localization.Text(LocalizationKeys.Settings.Language);
            languageBackText.text = localization.Text(LocalizationKeys.App.Back);

            for (var i = 0; i < LanguageOptions.Length; i++)
            {
                var isSelected = LanguageOptions[i].lang == localization.CurrentLanguage;
                var image = languageOptionButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    image.color = isSelected ? SelectedColor : UnselectedColor;
                }
            }
        }

        private void ShowSettingsPanel()
        {
            settingsPanel.gameObject.SetActive(true);
            languagePanel.gameObject.SetActive(false);
        }

        private void ShowLanguagePanel()
        {
            settingsPanel.gameObject.SetActive(false);
            languagePanel.gameObject.SetActive(true);
        }
    }
}
