using ChopChop.App;
using ChopChop.Audio;
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
        private AudioService audioService;
        private Text titleText;
        private Text languageLabelText;
        private Text bgmText;
        private Text backText;
        private Button[] languageOptionButtons;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator, AudioService audioService)
        {
            var root = UiFactory.CreateScreen("Settings Screen", new Color(0.045f, 0.05f, 0.05f, 1f), true);
            var controller = root.AddComponent<SettingsScreenController>();
            controller.Initialize(localization, navigator, audioService);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator, AudioService service)
        {
            localization = appLocalization;
            navigator = sceneNavigator;
            audioService = service;

            ApplyBackgroundImage();
            CreateReadableScrim();
            CreateLeftShade();
            CreatePanel();

            localization.LanguageChanged += UpdateTexts;
            if (audioService != null)
            {
                audioService.BgmSettingsChanged += UpdateTexts;
            }

            UpdateTexts();
        }

        private void CreatePanel()
        {
            var panel = UiFactory.CreatePanel(transform, "Settings Panel", new Color(0.025f, 0.02f, 0.015f, 0.72f));
            UiFactory.Anchor(panel, new Vector2(0.09f, 0.08f), new Vector2(0.48f, 0.92f), Vector2.zero, Vector2.zero);
            UiFactory.AddFadeSlideIn(panel.gameObject, 0.38f, 0.04f, new Vector2(-20f, 0f), 0.98f);
            UiFactory.AddVerticalLayout(panel.gameObject, 14f, TextAnchor.UpperLeft, new RectOffset(34, 34, 30, 30));

            titleText = UiFactory.CreateText(panel, "Title", 52, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetPreferredHeight(titleText.rectTransform, 68f);
            titleText.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);

            var rule = UiFactory.CreatePanel(panel, "Gold Rule", new Color(0.93f, 0.63f, 0.22f, 0.92f));
            UiFactory.SetPreferredHeight(rule, 2f);

            languageLabelText = UiFactory.CreateText(panel, "Language Label", 28, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.93f, 0.83f, 0.65f, 1f));
            UiFactory.SetPreferredHeight(languageLabelText.rectTransform, 38f);

            languageOptionButtons = new Button[LanguageOptions.Length];
            for (var i = 0; i < LanguageOptions.Length; i++)
            {
                var (lang, nativeLabel) = LanguageOptions[i];
                var btn = UiFactory.CreateButton(panel, $"Lang_{lang}", nativeLabel, UnselectedColor, new Color(0.98f, 0.92f, 0.82f, 1f));
                UiFactory.SetPreferredHeight(btn.GetComponent<RectTransform>(), 52f);
                var btnText = btn.GetComponentInChildren<Text>();
                btnText.alignment = TextAnchor.MiddleLeft;
                UiFactory.Anchor(btnText.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 0f), new Vector2(-16f, 0f));
                var capturedLang = lang;
                btn.onClick.AddListener(() => localization.SetLanguage(capturedLang));
                languageOptionButtons[i] = btn;
            }

            var secondRule = UiFactory.CreatePanel(panel, "Second Rule", new Color(0.93f, 0.63f, 0.22f, 0.40f));
            UiFactory.SetPreferredHeight(secondRule, 1f);

            var bgmButton = UiFactory.CreateButton(panel, "BGM Button", string.Empty, new Color(0.09f, 0.105f, 0.11f, 0.96f), new Color(0.95f, 0.89f, 0.78f, 1f));
            UiFactory.SetPreferredHeight(bgmButton.GetComponent<RectTransform>(), 62f);
            bgmText = bgmButton.GetComponentInChildren<Text>();
            bgmText.alignment = TextAnchor.MiddleLeft;
            UiFactory.Anchor(bgmText.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 0f), new Vector2(-16f, 0f));
            bgmButton.onClick.AddListener(() => audioService?.ToggleBgmEnabled());

            var spacer = new GameObject("Spacer", typeof(RectTransform));
            spacer.transform.SetParent(panel, false);
            UiFactory.SetFlexibleFill(spacer.GetComponent<RectTransform>());

            var backButton = UiFactory.CreateButton(panel, "Back Button", string.Empty, new Color(0.11f, 0.105f, 0.095f, 0.94f), new Color(0.95f, 0.89f, 0.78f, 1f));
            UiFactory.SetPreferredHeight(backButton.GetComponent<RectTransform>(), 56f);
            backText = backButton.GetComponentInChildren<Text>();
            backButton.onClick.AddListener(navigator.LoadSettingsReturn);
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

            if (audioService != null)
            {
                audioService.BgmSettingsChanged -= UpdateTexts;
            }
        }

        private void UpdateTexts()
        {
            titleText.text = localization.Text(LocalizationKeys.Settings.Title);
            languageLabelText.text = localization.Text(LocalizationKeys.Settings.Language);

            for (var i = 0; i < LanguageOptions.Length; i++)
            {
                var isSelected = LanguageOptions[i].lang == localization.CurrentLanguage;
                var image = languageOptionButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    image.color = isSelected ? SelectedColor : UnselectedColor;
                }
            }

            var bgmState = audioService == null || audioService.BgmEnabled
                ? localization.Text(LocalizationKeys.Settings.BgmOn)
                : localization.Text(LocalizationKeys.Settings.BgmOff);
            bgmText.text = $"{localization.Text(LocalizationKeys.Settings.Bgm)}    {bgmState}";
            backText.text = localization.Text(LocalizationKeys.App.Back);
        }
    }
}
