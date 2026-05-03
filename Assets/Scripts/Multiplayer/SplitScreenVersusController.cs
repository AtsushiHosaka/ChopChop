using ChopChop.App;
using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;

namespace ChopChop.Multiplayer
{
    [AddComponentMenu("ChopChop/Multiplayer/Split Screen Versus Controller")]
    public sealed class SplitScreenVersusController : MonoBehaviour
    {
        [SerializeField] private SplitScreenCameraLayout cameraLayout;
        [SerializeField] private SukiyakiStageController playerOneStage;
        [SerializeField] private SukiyakiStageController playerTwoStage;
        [SerializeField] private string retrySceneName = AppScenes.SukiyakiLevel;

        private bool playerOneComplete;
        private bool playerTwoComplete;
        private LevelRunResult playerOneResult;
        private LevelRunResult playerTwoResult;

        public void Configure(
            SplitScreenCameraLayout layout,
            SukiyakiStageController firstStage,
            SukiyakiStageController secondStage,
            string retryScene = AppScenes.SukiyakiLevel)
        {
            cameraLayout = layout;
            playerOneStage = firstStage;
            playerTwoStage = secondStage;
            retrySceneName = retryScene;
        }

        public void StartVersus()
        {
            playerOneComplete = false;
            playerTwoComplete = false;
            playerOneResult = default;
            playerTwoResult = default;

            cameraLayout?.ApplyLayout();
            playerOneStage?.StartLevel();
            playerTwoStage?.StartLevel();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (playerOneStage != null)
            {
                playerOneStage.Completed += HandlePlayerOneCompleted;
            }

            if (playerTwoStage != null)
            {
                playerTwoStage.Completed += HandlePlayerTwoCompleted;
            }
        }

        private void Unsubscribe()
        {
            if (playerOneStage != null)
            {
                playerOneStage.Completed -= HandlePlayerOneCompleted;
            }

            if (playerTwoStage != null)
            {
                playerTwoStage.Completed -= HandlePlayerTwoCompleted;
            }
        }

        private void HandlePlayerOneCompleted(LevelRunResult result)
        {
            playerOneResult = result;
            playerOneComplete = true;
            TryFinishVersus();
        }

        private void HandlePlayerTwoCompleted(LevelRunResult result)
        {
            playerTwoResult = result;
            playerTwoComplete = true;
            TryFinishVersus();
        }

        private void TryFinishVersus()
        {
            if (!playerOneComplete || !playerTwoComplete)
            {
                return;
            }

            var navigator = AppRuntime.Instance != null ? AppRuntime.Instance.Navigator : null;
            if (navigator == null)
            {
                Debug.LogWarning("[Split Screen Versus] Cannot load result because AppRuntime is missing.");
                return;
            }

            navigator.LoadVersusResult(new VersusLevelRunResult(playerOneResult, playerTwoResult), retrySceneName);
        }
    }
}
