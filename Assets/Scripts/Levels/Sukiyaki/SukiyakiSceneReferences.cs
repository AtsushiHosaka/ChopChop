using ChopChop.Food.Meat;
using ChopChop.Input;
using ChopChop.Presentation;
using UnityEngine;

namespace ChopChop.Levels.Sukiyaki
{
    [AddComponentMenu("ChopChop/Levels/Sukiyaki Scene References")]
    public sealed class SukiyakiSceneReferences : MonoBehaviour
    {
        [SerializeField] private SukiyakiStageController stageController;
        [SerializeField] private HashiInputProvider hashiInputProvider;
        [SerializeField] private WaypointRailMover railMover;
        [SerializeField] private MeatTearController meatTearController;

        public SukiyakiStageController StageController => stageController;
        public HashiInputProvider HashiInputProvider => hashiInputProvider;
        public WaypointRailMover RailMover => railMover;
        public MeatTearController MeatTearController => meatTearController;

        public bool IsReady => stageController != null && hashiInputProvider != null && railMover != null;

        private void Reset()
        {
            stageController = FindAnyObjectByType<SukiyakiStageController>();
            hashiInputProvider = FindAnyObjectByType<HashiInputProvider>();
            railMover = FindAnyObjectByType<WaypointRailMover>();
            meatTearController = FindAnyObjectByType<MeatTearController>();
        }
    }
}
