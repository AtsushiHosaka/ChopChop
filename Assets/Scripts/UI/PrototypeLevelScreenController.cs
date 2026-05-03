using ChopChop.App;
using ChopChop.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace ChopChop.UI
{
    public sealed class PrototypeLevelScreenController : MonoBehaviour
    {
        private GameLocalization localization;
        private SceneNavigator navigator;
        private LevelDefinition levelDefinition;
        private Text titleText;
        private Text descriptionText;
        private Text languageText;
        private Text levelSelectText;

        public static GameObject Create(GameLocalization localization, SceneNavigator navigator, LevelDefinition levelDefinition)
        {
            var root = UiFactory.CreateScreen("Prototype Level Screen", Color.clear, false);
            var controller = root.AddComponent<PrototypeLevelScreenController>();
            controller.Initialize(localization, navigator, levelDefinition);
            return root;
        }

        private void Initialize(GameLocalization appLocalization, SceneNavigator sceneNavigator, LevelDefinition definition)
        {
            localization = appLocalization;
            navigator = sceneNavigator;
            levelDefinition = definition;

            CreateTopBar();
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

        private void CreateTopBar()
        {
            var top = UiFactory.CreatePanel(transform, "Prototype HUD", new Color(0f, 0f, 0f, 0.42f));
            UiFactory.Anchor(top, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -116f), new Vector2(-24f, -20f));
            UiFactory.AddHorizontalLayout(top.gameObject, 18f, TextAnchor.MiddleLeft, new RectOffset(20, 20, 10, 10));

            var textGroup = new GameObject("Level Text", typeof(RectTransform));
            textGroup.transform.SetParent(top, false);
            var textRect = textGroup.GetComponent<RectTransform>();
            UiFactory.SetSize(textRect, 940f, 78f);
            UiFactory.AddVerticalLayout(textGroup, 2f, TextAnchor.MiddleLeft, new RectOffset(0, 0, 0, 0));

            titleText = UiFactory.CreateText(textGroup.transform, "Title", 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            UiFactory.SetSize(titleText.rectTransform, 920f, 40f);

            descriptionText = UiFactory.CreateText(textGroup.transform, "Description", 21, FontStyle.Normal, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.84f));
            UiFactory.SetSize(descriptionText.rectTransform, 920f, 32f);

            var languageButton = UiFactory.CreateButton(top, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 170f, 58f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var levelSelectButton = UiFactory.CreateButton(top, "Level Select Button", string.Empty, new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(levelSelectButton.GetComponent<RectTransform>(), 220f, 58f);
            levelSelectText = levelSelectButton.GetComponentInChildren<Text>();
            levelSelectButton.onClick.AddListener(navigator.LoadLevelSelect);
        }

        private void UpdateTexts()
        {
            if (levelDefinition != null)
            {
                titleText.text = localization.Text(levelDefinition.displayNameKey);
                descriptionText.text = localization.Text(levelDefinition.descriptionKey);
            }

            languageText.text = localization.Text(LocalizationKeys.App.Language);
            levelSelectText.text = localization.Text(LocalizationKeys.Result.LevelSelect);
        }
    }
}
