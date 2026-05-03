using UnityEngine;

namespace ChopChop.Levels
{
    [CreateAssetMenu(menuName = "ChopChop/Levels/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public string levelId = "level";
        public string displayNameKey = "level.name";
        public string descriptionKey = "level.description";
        public string sceneName = string.Empty;
        [Min(1)] public int difficulty = 1;
        public bool isUnlocked;
        public Sprite thumbnail;
    }
}
