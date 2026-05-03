using ChopChop.Levels;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;

namespace ChopChop.Audio
{
    [DisallowMultipleComponent]
    public sealed class SukiyakiAudioBinder : MonoBehaviour
    {
        private SukiyakiStageController stageController;

        public void Initialize(SukiyakiStageController stage)
        {
            Unsubscribe();
            stageController = stage;
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (stageController == null)
            {
                return;
            }

            stageController.StepChanged += HandleStepChanged;
            stageController.StepSucceeded += HandleStepSucceeded;
            stageController.StepFailed += HandleStepFailed;
            stageController.Completed += HandleCompleted;
        }

        private void Unsubscribe()
        {
            if (stageController == null)
            {
                return;
            }

            stageController.StepChanged -= HandleStepChanged;
            stageController.StepSucceeded -= HandleStepSucceeded;
            stageController.StepFailed -= HandleStepFailed;
            stageController.Completed -= HandleCompleted;
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            if (step != SukiyakiStep.Calibration && step != SukiyakiStep.Result)
            {
                AudioService.Instance?.PlayCue(AudioCueId.StepChanged);
            }
        }

        private void HandleStepSucceeded(SukiyakiStep step)
        {
            if (step != SukiyakiStep.Calibration && step != SukiyakiStep.Result)
            {
                AudioService.Instance?.PlayCue(AudioCueId.StepSuccess);
            }
        }

        private void HandleStepFailed(SukiyakiStep step)
        {
            AudioService.Instance?.PlayCue(AudioCueId.StepFailure);
        }

        private void HandleCompleted(LevelRunResult result)
        {
            AudioService.Instance?.PlayCue(AudioCueId.LevelComplete);
        }
    }
}
