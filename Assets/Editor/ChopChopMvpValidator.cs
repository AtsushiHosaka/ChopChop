using ChopChop.App;
using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChopChop.Editor
{
    public static class ChopChopMvpValidator
    {
        [MenuItem("ChopChop/Validate MVP Setup")]
        public static void Validate()
        {
            var hasError = false;
            hasError |= !ValidateBuildScenes();
            hasError |= !ValidateLevelCatalog();
            hasError |= !ValidateLocalizationSource();
            hasError |= !ValidateSukiyakiSceneReferences();
            hasError |= !ValidateReleaseDemoModes();

            if (hasError)
            {
                Debug.LogError("[ChopChop] MVP setup validation failed.");
                return;
            }

            Debug.Log("[ChopChop] MVP setup validation passed.");
        }

        private static bool ValidateBuildScenes()
        {
            var expected = new[]
            {
                "Assets/Scenes/StartScene.unity",
                "Assets/Scenes/LevelSelectScene.unity",
                "Assets/Scenes/Levels/Tutorial_TempuraScene.unity",
                "Assets/Scenes/Levels/Level01_SashimiScene.unity",
                "Assets/Scenes/Levels/Level02_TamagoKakeGohanScene.unity",
                "Assets/Scenes/Levels/Level03_TofuScene.unity",
                "Assets/Scenes/Levels/Level04_GrilledFishScene.unity",
                "Assets/Scenes/Levels/Level05_SukiyakiScene.unity",
                "Assets/Scenes/ResultScene.unity"
            };

            var scenes = EditorBuildSettings.scenes;
            if (scenes.Length < expected.Length)
            {
                Debug.LogError($"[ChopChop] Build Settings has {scenes.Length} scenes. Expected at least {expected.Length}.");
                return false;
            }

            for (var i = 0; i < expected.Length; i++)
            {
                if (!scenes[i].enabled || scenes[i].path != expected[i])
                {
                    Debug.LogError($"[ChopChop] Build Settings scene {i} should be '{expected[i]}', but was '{scenes[i].path}'.");
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateLevelCatalog()
        {
            var catalog = Resources.Load<LevelCatalog>("Configs/Levels/LevelCatalog");
            if (catalog == null)
            {
                Debug.LogError("[ChopChop] LevelCatalog was not found at Resources/Configs/Levels/LevelCatalog.");
                return false;
            }

            var expectedScenes = new[]
            {
                AppScenes.TutorialTempura,
                AppScenes.Level01Sashimi,
                AppScenes.Level02TamagoKakeGohan,
                AppScenes.Level03Tofu,
                AppScenes.Level04GrilledFish,
                AppScenes.SukiyakiLevel
            };

            if (catalog.Levels.Count < expectedScenes.Length)
            {
                Debug.LogError($"[ChopChop] LevelCatalog has {catalog.Levels.Count} levels. Expected at least {expectedScenes.Length}.");
                return false;
            }

            for (var i = 0; i < expectedScenes.Length; i++)
            {
                var level = catalog.Levels[i];
                if (level == null)
                {
                    Debug.LogError($"[ChopChop] LevelCatalog entry {i} is missing.");
                    return false;
                }

                if (level.sceneName != expectedScenes[i])
                {
                    Debug.LogError($"[ChopChop] LevelCatalog entry {i} should point to '{expectedScenes[i]}', but was '{level.sceneName}'.");
                    return false;
                }

                if (!level.isUnlocked)
                {
                    Debug.LogError($"[ChopChop] LevelCatalog entry {i} for '{level.sceneName}' must be unlocked for the current multi-level prototype.");
                    return false;
                }

                if (level.thumbnail == null)
                {
                    Debug.LogError($"[ChopChop] LevelCatalog entry {i} for '{level.sceneName}' has no thumbnail.");
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateLocalizationSource()
        {
            foreach (var key in LocalizationCatalog.Japanese.Keys)
            {
                if (!LocalizationCatalog.English.ContainsKey(key))
                {
                    Debug.LogError($"[ChopChop] English localization is missing key '{key}'.");
                    return false;
                }
            }

            foreach (var key in LocalizationCatalog.English.Keys)
            {
                if (!LocalizationCatalog.Japanese.ContainsKey(key))
                {
                    Debug.LogError($"[ChopChop] Japanese localization is missing key '{key}'.");
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateSukiyakiSceneReferences()
        {
            const string scenePath = "Assets/Scenes/Levels/Level05_SukiyakiScene.unity";
            var openedAdditively = false;
            var scene = SceneManager.GetSceneByPath(scenePath);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                openedAdditively = true;
            }

            try
            {
                var references = FindInScene<SukiyakiSceneReferences>(scene);
                if (references == null)
                {
                    Debug.LogError("[ChopChop] Level05_SukiyakiScene needs one SukiyakiSceneReferences component.");
                    return false;
                }

                if (!references.IsReady)
                {
                    Debug.LogError("[ChopChop] SukiyakiSceneReferences must assign StageController, HashiInputProvider, and RailMover before the production MVP can run.");
                    return false;
                }

                if (references.MeatTearController == null)
                {
                    Debug.LogWarning("[ChopChop] SukiyakiSceneReferences has no MeatTearController. This is acceptable only before the Obi meat prefab is connected.");
                    return true;
                }

                if (references.MeatTearController.TearProfile == null)
                {
                    Debug.LogError("[ChopChop] Sukiyaki MeatTearController must assign a MeatTearProfile.");
                    return false;
                }

                if (references.MeatTearController.TearDriver == null)
                {
                    Debug.LogError("[ChopChop] Sukiyaki MeatTearController must assign a tear driver.");
                    return false;
                }

                var layout = FindInScene<SukiyakiStageLayout>(scene);
                if (layout == null)
                {
                    Debug.LogError("[ChopChop] Level05_SukiyakiScene needs one SukiyakiStageLayout component for editable stage coordinates.");
                    return false;
                }

                if (layout.Egg.StationRoot == null ||
                    layout.RawMeat.StationRoot == null ||
                    layout.Broth.StationRoot == null ||
                    layout.Flip.StationRoot == null ||
                    layout.Mouth.StationRoot == null)
                {
                    Debug.LogError("[ChopChop] SukiyakiStageLayout must assign Egg, Raw Meat, Broth, Flip, and Mouth station roots.");
                    return false;
                }

                foreach (var waypoint in layout.GetOrderedWaypoints())
                {
                    if (waypoint == null)
                    {
                        Debug.LogError("[ChopChop] SukiyakiStageLayout must assign all rail waypoints.");
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                if (openedAdditively && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static bool ValidateReleaseDemoModes()
        {
            var allValid = true;
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled || string.IsNullOrWhiteSpace(buildScene.path))
                {
                    continue;
                }

                var openedAdditively = false;
                var scene = SceneManager.GetSceneByPath(buildScene.path);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Additive);
                    openedAdditively = true;
                }

                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        foreach (var demoDriver in root.GetComponentsInChildren<SukiyakiDemoDriver>(true))
                        {
                            if (!demoDriver.DemoModeEnabled)
                            {
                                continue;
                            }

                            Debug.LogError($"[ChopChop] Demo driver '{demoDriver.name}' is enabled in build scene '{buildScene.path}'. Production/release validation requires demo mode OFF.");
                            allValid = false;
                        }
                    }
                }
                finally
                {
                    if (openedAdditively && scene.IsValid() && scene.isLoaded)
                    {
                        EditorSceneManager.CloseScene(scene, true);
                    }
                }
            }

            return allValid;
        }

        private static T FindInScene<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
