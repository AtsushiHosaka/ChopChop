using System.Collections.Generic;
using UnityEngine;

namespace ChopChop.Levels
{
    [CreateAssetMenu(menuName = "ChopChop/Levels/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private LevelDefinition[] levels = new LevelDefinition[0];

        public IReadOnlyList<LevelDefinition> Levels => levels;
    }
}
