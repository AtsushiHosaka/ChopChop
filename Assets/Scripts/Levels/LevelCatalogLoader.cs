using ChopChop.App;
using UnityEngine;

namespace ChopChop.Levels
{
    public static class LevelCatalogLoader
    {
        private const string ResourcesPath = "Configs/Levels/LevelCatalog";

        public static LevelCatalog Load()
        {
            var catalog = Resources.Load<LevelCatalog>(ResourcesPath);
            return catalog != null ? catalog : CreateFallback();
        }

        private static LevelCatalog CreateFallback()
        {
            var catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            var serialized = new UnityEditorFallbackLevelCatalog(catalog);
            serialized.SetLevels(new[]
            {
                Create("level-1", "level.1.name", "level.1.desc", string.Empty, 1, false),
                Create("level-2", "level.2.name", "level.2.desc", string.Empty, 2, false),
                Create("level-3", "level.3.name", "level.3.desc", string.Empty, 3, false),
                Create("level-4", "level.4.name", "level.4.desc", string.Empty, 4, false),
                Create("level-5", "level.5.name", "level.5.desc", AppScenes.SukiyakiLevel, 5, true)
            });
            return catalog;
        }

        private static LevelDefinition Create(string id, string nameKey, string descriptionKey, string sceneName, int difficulty, bool unlocked)
        {
            var definition = ScriptableObject.CreateInstance<LevelDefinition>();
            definition.levelId = id;
            definition.displayNameKey = nameKey;
            definition.descriptionKey = descriptionKey;
            definition.sceneName = sceneName;
            definition.difficulty = difficulty;
            definition.isUnlocked = unlocked;
            return definition;
        }

        private sealed class UnityEditorFallbackLevelCatalog
        {
            private readonly LevelCatalog catalog;

            public UnityEditorFallbackLevelCatalog(LevelCatalog catalog)
            {
                this.catalog = catalog;
            }

            public void SetLevels(LevelDefinition[] levels)
            {
                var field = typeof(LevelCatalog).GetField("levels", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                field?.SetValue(catalog, levels);
            }
        }
    }
}
