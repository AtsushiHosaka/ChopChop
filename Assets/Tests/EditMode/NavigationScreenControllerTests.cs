using System.Collections.Generic;
using ChopChop.App;
using ChopChop.Levels;
using ChopChop.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChopChop.Tests.EditMode
{
    public sealed class NavigationScreenControllerTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            foreach (var eventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include))
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }

            createdObjects.Clear();
        }

        [Test]
        public void StartScreenCreatesStartSettingsAndQuitButtons()
        {
            var root = StartScreenController.Create(new GameLocalization(), new SceneNavigator());
            createdObjects.Add(root);

            AssertButtonExists(root, "Start Button");
            AssertButtonExists(root, "Settings Button");
            AssertButtonExists(root, "Quit Button");
        }

        [Test]
        public void ResultScreenCreatesRetryBackAndLanguageButtons()
        {
            var result = new LevelRunResult(12.5f, 1, 80, 92, true);
            var root = ResultScreenController.Create(new GameLocalization(), new SceneNavigator(), result, AppScenes.SukiyakiLevel);
            createdObjects.Add(root);

            AssertButtonExists(root, "Retry Button");
            AssertButtonExists(root, "Back Button");
            AssertButtonExists(root, "Language Button");
        }

        [Test]
        public void LevelSelectScreenCreatesBackSettingsAndLevelCards()
        {
            var root = LevelSelectScreenController.Create(new GameLocalization(), new SceneNavigator(), LevelCatalogLoader.Load());
            createdObjects.Add(root);

            AssertButtonExists(root, "Back Button");
            AssertButtonExists(root, "Settings Button");
            Assert.That(CountLevelButtons(root), Is.GreaterThanOrEqualTo(5));
            Assert.That(root.transform.Find("Level Menu Panel/Level Viewport/Level Grid")?.GetComponent<GridLayoutGroup>(), Is.Not.Null);
            Assert.That(root.transform.Find("Level Menu Panel/Level Viewport/Level Grid")?.GetComponent<UiResponsiveGridLayout>(), Is.Not.Null);
        }

        [Test]
        public void SettingsScreenCreatesLanguageAndBackButtons()
        {
            var root = SettingsScreenController.Create(new GameLocalization(), new SceneNavigator());
            createdObjects.Add(root);

            AssertButtonExists(root, "Language Button");
            AssertButtonExists(root, "Back Button");
        }

        private static void AssertButtonExists(GameObject root, string name)
        {
            var button = FindButton(root, name);
            Assert.That(button, Is.Not.Null, $"{name} should exist.");
            Assert.That(button.interactable, Is.True, $"{name} should be interactable.");
            Assert.That(button.GetComponent<RectTransform>().rect.height, Is.GreaterThan(0f), $"{name} should have visible height.");
        }

        private static Button FindButton(GameObject root, string name)
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name == name)
                {
                    return button;
                }
            }

            return null;
        }

        private static int CountLevelButtons(GameObject root)
        {
            var count = 0;
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name.StartsWith("level") || button.name.StartsWith("tutorial"))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
