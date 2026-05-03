using ChopChop.Input;
using UnityEngine;

namespace ChopChop.Levels.Sukiyaki
{
    [AddComponentMenu("ChopChop/Levels/Sukiyaki Demo Driver")]
    public sealed class SukiyakiDemoDriver : MonoBehaviour
    {
        [SerializeField] private bool demoModeEnabled;
        [SerializeField] private bool autoAdvanceSteps = true;
        [SerializeField] private HashiInputProvider inputProvider;
        [SerializeField] private SukiyakiStageController stageController;
        [SerializeField] private float calibrationSeconds = 3.2f;
        [SerializeField] private float mixEggSeconds = 3.4f;
        [SerializeField] private float pickMeatSeconds = 1.4f;
        [SerializeField] private float putInBrothSeconds = 2f;
        [SerializeField] private float flipMeatSeconds = 2.4f;
        [SerializeField] private float dipEggSeconds = 1.7f;
        [SerializeField] private float deliverToMouthSeconds = 1.8f;

        private SukiyakiStep observedStep;
        private float stepStartedAt;

        public bool DemoModeEnabled => demoModeEnabled;

        public void SetDemoModeEnabled(bool enabled)
        {
            demoModeEnabled = enabled;
            ApplyInputMode();
        }

        private void Awake()
        {
            ResolveReferences();
            observedStep = stageController != null ? stageController.CurrentStep : SukiyakiStep.Calibration;
            stepStartedAt = Time.time;
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (stageController != null)
            {
                stageController.StepChanged += HandleStepChanged;
            }

            ApplyInputMode();
            HandleStepChanged(stageController != null ? stageController.CurrentStep : SukiyakiStep.Calibration);
        }

        private void OnDisable()
        {
            if (stageController != null)
            {
                stageController.StepChanged -= HandleStepChanged;
            }
        }

        private void Update()
        {
            if (!demoModeEnabled)
            {
                return;
            }

            ResolveReferences();
            ApplyInputMode();

            if (stageController == null || inputProvider == null)
            {
                return;
            }

            if (stageController.CurrentStep != observedStep)
            {
                HandleStepChanged(stageController.CurrentStep);
            }

            inputProvider.SetDemoPhase(PhaseFor(stageController.CurrentStep));

            if (!autoAdvanceSteps || !stageController.IsRunning || stageController.CurrentStep == SukiyakiStep.Result)
            {
                return;
            }

            if (Time.time - stepStartedAt >= DurationFor(stageController.CurrentStep))
            {
                stageController.CompleteCurrentStep();
            }
        }

        private void ResolveReferences()
        {
            if (inputProvider == null)
            {
                inputProvider = FindAnyObjectByType<HashiInputProvider>();
            }

            if (stageController == null)
            {
                stageController = FindAnyObjectByType<SukiyakiStageController>();
            }
        }

        private void ApplyInputMode()
        {
            if (inputProvider == null)
            {
                return;
            }

            inputProvider.SetInputMode(demoModeEnabled ? HashiInputMode.Demo : HashiInputMode.Ble);
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            observedStep = step;
            stepStartedAt = Time.time;

            if (demoModeEnabled && inputProvider != null)
            {
                inputProvider.SetDemoPhase(PhaseFor(step));
            }
        }

        private float DurationFor(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.Calibration:
                    return calibrationSeconds;
                case SukiyakiStep.MixEgg:
                    return mixEggSeconds;
                case SukiyakiStep.PickMeat:
                    return pickMeatSeconds;
                case SukiyakiStep.PutInBroth:
                    return putInBrothSeconds;
                case SukiyakiStep.FlipMeat:
                    return flipMeatSeconds;
                case SukiyakiStep.DipEgg:
                    return dipEggSeconds;
                case SukiyakiStep.DeliverToMouth:
                    return deliverToMouthSeconds;
                default:
                    return float.PositiveInfinity;
            }
        }

        private static HashiDemoPhase PhaseFor(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.MixEgg:
                    return HashiDemoPhase.MixEgg;
                case SukiyakiStep.PickMeat:
                    return HashiDemoPhase.PickMeat;
                case SukiyakiStep.PutInBroth:
                    return HashiDemoPhase.CarryToBroth;
                case SukiyakiStep.FlipMeat:
                    return HashiDemoPhase.FlipMeat;
                case SukiyakiStep.DipEgg:
                    return HashiDemoPhase.DipEgg;
                case SukiyakiStep.DeliverToMouth:
                    return HashiDemoPhase.DeliverToMouth;
                default:
                    return HashiDemoPhase.Calibration;
            }
        }
    }
}
