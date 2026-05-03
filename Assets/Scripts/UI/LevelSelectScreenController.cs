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

            var header = UiFactory.CreatePanel(transform, "Header", new Color(0f, 0f, 0f, 0.2f));
            UiFactory.Anchor(header, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(44f, -126f), new Vector2(-44f, -28f));
            UiFactory.AddHorizontalLayout(header.gameObject, 20f, TextAnchor.MiddleLeft, new RectOffset(24, 24, 12, 12));

            titleText = UiFactory.CreateText(header, "Title", 48, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            titleText.rectTransform.sizeDelta = new Vector2(900f, 78f);

            var languageButton = UiFactory.CreateButton(header, "Language Button", string.Empty, new Color(0.16f, 0.48f, 0.62f, 1f), Color.white);
            UiFactory.SetSize(languageButton.GetComponent<RectTransform>(), 220f, 60f);
            languageText = languageButton.GetComponentInChildren<Text>();
            languageButton.onClick.AddListener(localization.ToggleLanguage);

            var backButton = UiFactory.CreateButton(header, "Back Button", string.Empty, new Color(0.19f, 0.21f, 0.22f, 1f), Color.white);
            UiFactory.SetSize(backButton.GetComponent<RectTransform>(), 180f, 60f);
            backText = backButton.GetComponentInChildren<Text>();
            backButton.onClick.AddListener(navigator.LoadStart);

            var gridObject = new GameObject("Level Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(transform, false);
            var gridRect = gridObject.GetComponent<RectTransform>();
            UiFactory.Anchor(gridRect, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(80f, 90f), new Vector2(-80f, -170f));

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
            var color = definition.isUnlocked
                ? new Color(0.95f, 0.32f, 0.22f, 1f)
                : new Color(0.22f, 0.24f, 0.25f, 1f);
            var button = UiFactory.CreateButton(parent, definition.levelId, string.Empty, color, Color.white);
            button.interactable = definition.isUnlocked;

            var label = button.GetComponentInChildren<Text>();
            label.fontSize = 34;
            label.alignment = TextAnchor.UpperLeft;
            UiFactory.Anchor(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(26f, 104f), new Vector2(-26f, -22f));

            var description = UiFactory.CreateText(button.transform, "Description", 23, FontStyle.Normal, TextAnchor.UpperLeft, new Color(1f, 1f, 1f, 0.86f));
            UiFactory.Anchor(description.rectTransform, Vector2.zero, Vector2.one, new Vector2(26f, 40f), new Vector2(-26f, -94f));

            var state = UiFactory.CreateText(button.transform, "State", 22, FontStyle.Bold, TextAnchor.LowerRight, new Color(1f, 1f, 1f, 0.8f));
            UiFactory.Anchor(state.rectTransform, Vector2.zero, Vector2.one, new Vector2(26f, 20f), new Vector2(-26f, -20f));

            button.onClick.AddListener(() => navigator.LoadLevel(definition.sceneName));
            return new LevelCard(definition, label, description, state);
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
