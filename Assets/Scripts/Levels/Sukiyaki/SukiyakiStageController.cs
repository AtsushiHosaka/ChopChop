using System;
using ChopChop.Food.Liquid;
using ChopChop.Input;
using ChopChop.Food.Meat;
using ChopChop.Presentation;
using ChopChop.Levels;
using UnityEngine;

namespace ChopChop.Levels.Sukiyaki
{
    public enum HashiStageMovementMode
    {
        Auto,
        DemoWaypointRail,
        HardwarePositionOnly,
        Hybrid
    }

    [AddComponentMenu("ChopChop/Levels/Sukiyaki Stage Controller")]
    public sealed class SukiyakiStageController : MonoBehaviour, ILevelController
    {
        // 食べるジェスチャーの2フェーズ
        private enum EatGesturePhase { WaitingForLift, WaitingForPullback }

        [SerializeField] private float calibrationSeconds = 3f;
        [SerializeField] private MeatTearController meatTearController;
        [SerializeField] private HashiInputProvider inputProvider;
        [SerializeField] private WaypointRailMover railMover;
        [SerializeField] private LiquidSurfacePresenter eggLiquidSurface;
        [SerializeField] private LiquidSurfacePresenter brothLiquidSurface;
        [SerializeField] private ObiFluidMixMonitor eggFluidMixMonitor;
        [SerializeField] private SukiyakiStepTargetZone[] targetZones = Array.Empty<SukiyakiStepTargetZone>();
        [SerializeField] private bool automaticProgress = true;
        [SerializeField] private HashiStageMovementMode hashiMovementMode = HashiStageMovementMode.Auto;

        // DeliverToMouth: 上げて引く食べるジェスチャーの閾値（Inspectorで調整可）
        // worldAccelY の単位はg（静止時 ≈ +1.0g）
        [SerializeField] private float liftAccelThreshold = 0.0f;
        [SerializeField] private float pullbackAccelThreshold = -1.5f;
        [SerializeField] private float gestureLiftResetSeconds = 3.0f;

        private float levelStartTime;
        private float calibrationStartTime;
        private float roughness;
        private float stepProgress;
        private float stepGoalSeconds = 1f;
        private LevelRunResult result;

        private EatGesturePhase eatGesturePhase;
        private float liftDetectedAt;

        public bool IsRunning { get; private set; }
        public SukiyakiStep CurrentStep { get; private set; } = SukiyakiStep.Calibration;
        public float ElapsedSeconds => IsRunning ? Time.time - levelStartTime : result.CompletionTime;
        public int FailureCount { get; private set; }
        public float StepProgress01 => stepGoalSeconds <= 0f ? 0f : Mathf.Clamp01(stepProgress / stepGoalSeconds);
        public LevelRunResult CurrentResult => result;
        public bool IsCurrentTargetSatisfied => IsTargetSatisfiedFor(CurrentStep);

        public event Action<SukiyakiStep> StepChanged;
        public event Action<SukiyakiStep> StepSucceeded;
        public event Action<SukiyakiStep> StepFailed;
        public event Action<LevelRunResult> Completed;

        public void Configure(HashiInputProvider hashiInput, WaypointRailMover mover, MeatTearController tearController)
        {
            inputProvider = hashiInput;
            railMover = mover;
            meatTearController = tearController;
        }

        public void StartLevel()
        {
            if (meatTearController != null)
            {
                meatTearController.Torn -= HandleMeatTorn;
            }

            levelStartTime = Time.time;
            calibrationStartTime = Time.time;
            roughness = 0f;
            FailureCount = 0;
            IsRunning = true;
            result = new LevelRunResult(0f, 0, 0, 0, false);
            SetStep(SukiyakiStep.Calibration);
            UpdateLiquidVisuals();

            if (meatTearController != null)
            {
                meatTearController.SetLevelStartTime(levelStartTime);
                meatTearController.Torn += HandleMeatTorn;
            }
        }

        public void RestartLevel()
        {
            if (meatTearController != null)
            {
                meatTearController.Torn -= HandleMeatTorn;
                meatTearController.ResetMeat();
            }

            StartLevel();
        }

        public void CompleteCurrentStep()
        {
            if (!IsRunning)
            {
                return;
            }

            var completedStep = CurrentStep;
            switch (CurrentStep)
            {
                case SukiyakiStep.Calibration:
                    StepSucceeded?.Invoke(completedStep);
                    SetStep(SukiyakiStep.MixEgg);
                    break;
                case SukiyakiStep.MixEgg:
                    StepSucceeded?.Invoke(completedStep);
                    SetStep(SukiyakiStep.PickMeat);
                    break;
                case SukiyakiStep.PickMeat:
                    StepSucceeded?.Invoke(completedStep);
                    SetStep(SukiyakiStep.PutInBroth);
                    break;
                case SukiyakiStep.PutInBroth:
                    StepSucceeded?.Invoke(completedStep);
                    SetStep(SukiyakiStep.FlipMeat);
                    break;
                case SukiyakiStep.FlipMeat:
                    StepSucceeded?.Invoke(completedStep);
                    SetStep(SukiyakiStep.DipEgg);
                    break;
                case SukiyakiStep.DipEgg:
                    StepSucceeded?.Invoke(completedStep);
                    SetStep(SukiyakiStep.DeliverToMouth);
                    break;
                case SukiyakiStep.DeliverToMouth:
                    StepSucceeded?.Invoke(completedStep);
                    FinishLevel();
                    break;
            }
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            if (CurrentStep == SukiyakiStep.Calibration && Time.time - calibrationStartTime >= calibrationSeconds)
            {
                CompleteCurrentStep();
            }

            if (meatTearController != null)
            {
                roughness = Mathf.Max(roughness, meatTearController.Damage / 100f);
            }

            if (automaticProgress && inputProvider != null && CurrentStep != SukiyakiStep.Calibration)
            {
                EvaluateStepInput(inputProvider.CurrentFrame, Time.deltaTime);
            }

            UpdateLiquidVisuals();
        }

        private void OnDestroy()
        {
            if (meatTearController != null)
            {
                meatTearController.Torn -= HandleMeatTorn;
            }
        }

        private void HandleMeatTorn(MeatTearEvent tearEvent)
        {
            FailureCount++;
            roughness += 0.25f;

            if (CurrentStep == SukiyakiStep.PutInBroth || CurrentStep == SukiyakiStep.FlipMeat)
            {
                var failedStep = CurrentStep;
                StepFailed?.Invoke(failedStep);
                meatTearController?.ResetMeat();
                SetStep(failedStep);
            }
        }

        private void FinishLevel()
        {
            var completionTime = Time.time - levelStartTime;
            result = new LevelRunResult(
                completionTime,
                FailureCount,
                SukiyakiScoreCalculator.Speed(completionTime),
                SukiyakiScoreCalculator.Care(FailureCount, roughness),
                true);
            IsRunning = false;
            SetStep(SukiyakiStep.Result);
            Completed?.Invoke(result);
        }

        private void SetStep(SukiyakiStep step)
        {
            CurrentStep = step;
            stepProgress = 0f;
            stepGoalSeconds = GoalSecondsFor(step);
            if (step == SukiyakiStep.DeliverToMouth)
            {
                eatGesturePhase = EatGesturePhase.WaitingForLift;
            }
            UpdateMeatStateFor(step);
            MoveRailFor(step);
            UpdateLiquidVisuals();
            StepChanged?.Invoke(CurrentStep);
        }

        private void UpdateLiquidVisuals()
        {
            if (eggLiquidSurface != null)
            {
                var eggMix01 = eggFluidMixMonitor != null && eggFluidMixMonitor.HasActiveEggFluid
                    ? eggFluidMixMonitor.EggMix01
                    : StepProgress01;

                if (CurrentStep == SukiyakiStep.MixEgg)
                {
                    eggLiquidSurface.SetMix(eggMix01);
                    eggLiquidSurface.SetActivity(Mathf.Lerp(0.2f, 1f, eggMix01));
                }
                else
                {
                    eggLiquidSurface.SetMix(CurrentStep > SukiyakiStep.MixEgg ? 1f : 0f);
                    eggLiquidSurface.SetActivity(CurrentStep == SukiyakiStep.DipEgg ? 0.5f : 0.08f);
                }
            }

            if (brothLiquidSurface != null)
            {
                var brothActivity = 0.05f;
                if (CurrentStep == SukiyakiStep.PutInBroth)
                {
                    brothActivity = Mathf.Lerp(0.25f, 1f, StepProgress01);
                }
                else if (CurrentStep == SukiyakiStep.FlipMeat)
                {
                    brothActivity = 0.35f;
                }

                brothLiquidSurface.SetActivity(brothActivity);
            }
        }

        private void UpdateMeatStateFor(SukiyakiStep step)
        {
            if (meatTearController == null || meatTearController.IsTorn)
            {
                return;
            }

            switch (step)
            {
                case SukiyakiStep.PickMeat:
                    meatTearController.SetMeatState(MeatState.Gripped);
                    break;
                case SukiyakiStep.PutInBroth:
                    meatTearController.SetMeatState(MeatState.FlipReady);
                    break;
                case SukiyakiStep.FlipMeat:
                    meatTearController.SetMeatState(MeatState.Flipping);
                    break;
                case SukiyakiStep.DipEgg:
                    meatTearController.SetMeatState(MeatState.Dipped);
                    break;
                case SukiyakiStep.DeliverToMouth:
                    meatTearController.SetMeatState(MeatState.Delivered);
                    break;
            }
        }

        private void EvaluateStepInput(HashiInputFrame frame, float deltaTime)
        {
            if (!frame.HasSample || CurrentStep == SukiyakiStep.Result)
            {
                return;
            }

            roughness = Mathf.Max(roughness, Roughness(frame));

            if (CurrentStep == SukiyakiStep.DeliverToMouth)
            {
                EvaluateEatGesture(frame);
                return;
            }

            var quality = QualityFor(CurrentStep, frame);
            var targetSatisfied = IsTargetSatisfiedFor(CurrentStep);
            if (CurrentStep == SukiyakiStep.MixEgg && eggFluidMixMonitor != null && eggFluidMixMonitor.HasActiveEggFluid)
            {
                stepProgress = targetSatisfied
                    ? Mathf.Clamp01(eggFluidMixMonitor.EggMix01) * stepGoalSeconds
                    : Mathf.Max(0f, stepProgress - deltaTime * 0.35f);
            }
            else
            {
                var progressDelta = targetSatisfied && quality >= 0.55f ? deltaTime * quality : -deltaTime * 0.35f;
                stepProgress = Mathf.Clamp(stepProgress + progressDelta, 0f, stepGoalSeconds);
            }

            if (stepProgress >= stepGoalSeconds)
            {
                CompleteCurrentStep();
            }
        }

        private static float GoalSecondsFor(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.MixEgg:
                    return 2.6f;
                case SukiyakiStep.PickMeat:
                    return 0.8f;
                case SukiyakiStep.PutInBroth:
                    return 1.4f;
                case SukiyakiStep.FlipMeat:
                    return 1.8f;
                case SukiyakiStep.DipEgg:
                    return 1.2f;
                case SukiyakiStep.DeliverToMouth:
                    return 1.2f;
                default:
                    return 1f;
            }
        }

        private static float QualityFor(SukiyakiStep step, HashiInputFrame frame)
        {
            var gyro = frame.GyroDegreesPerSecond.magnitude;
            switch (step)
            {
                case SukiyakiStep.MixEgg:
                    return Mathf.InverseLerp(3f, 8f, gyro) * SafeOpen(frame.ThetaDegrees);
                case SukiyakiStep.PickMeat:
                    return Closed(frame.ThetaDegrees);
                case SukiyakiStep.PutInBroth:
                    return Mathf.Max(Closed(frame.ThetaDegrees), frame.PinchNormalized);
                case SukiyakiStep.FlipMeat:
                    return Mathf.InverseLerp(5f, 12f, gyro) * Mathf.Max(Closed(frame.ThetaDegrees), 0.65f);
                case SukiyakiStep.DipEgg:
                case SukiyakiStep.DeliverToMouth:
                    return Mathf.Max(Closed(frame.ThetaDegrees), frame.PinchNormalized);
                default:
                    return 0f;
            }
        }

        private static float Closed(float theta)
        {
            return 1f - Mathf.InverseLerp(10f, 26f, theta);
        }

        private static float SafeOpen(float theta)
        {
            return 1f - Mathf.Abs(Mathf.InverseLerp(8f, 44f, theta) - 0.5f) * 2f;
        }

        private void EvaluateEatGesture(HashiInputFrame frame)
        {
            // デバイス座標系の加速度を世界座標系に変換してY成分を取得
            var worldAccelY = (frame.LocalRotation * frame.Acceleration).y;

            switch (eatGesturePhase)
            {
                case EatGesturePhase.WaitingForLift:
                    if (worldAccelY > liftAccelThreshold)
                    {
                        eatGesturePhase = EatGesturePhase.WaitingForPullback;
                        liftDetectedAt = Time.time;
                        stepProgress = stepGoalSeconds * 0.5f;
                    }
                    break;

                case EatGesturePhase.WaitingForPullback:
                    if (Time.time - liftDetectedAt > gestureLiftResetSeconds)
                    {
                        eatGesturePhase = EatGesturePhase.WaitingForLift;
                        stepProgress = 0f;
                        break;
                    }
                    if (worldAccelY < pullbackAccelThreshold)
                    {
                        CompleteCurrentStep();
                    }
                    break;
            }
        }

        private static float Roughness(HashiInputFrame frame)
        {
            var angularRoughness = Mathf.InverseLerp(80f, 360f, frame.GyroDegreesPerSecond.magnitude);
            var accelerationRoughness = Mathf.InverseLerp(2f, 8f, frame.Acceleration.magnitude);
            return Mathf.Clamp01(Mathf.Max(angularRoughness, accelerationRoughness));
        }

        public bool IsTargetSatisfiedFor(SukiyakiStep step)
        {
            if (targetZones == null || targetZones.Length == 0)
            {
                return true;
            }

            var hasZoneForStep = false;
            for (var i = 0; i < targetZones.Length; i++)
            {
                var zone = targetZones[i];
                if (zone == null || zone.Step != step)
                {
                    continue;
                }

                hasZoneForStep = true;
                if (zone.isActiveAndEnabled && zone.IsSatisfied)
                {
                    return true;
                }
            }

            return !hasZoneForStep;
        }

        private void MoveRailFor(SukiyakiStep step)
        {
            if (railMover == null || !ShouldMoveDemoRail())
            {
                return;
            }

            switch (step)
            {
                case SukiyakiStep.Calibration:
                    railMover.MoveTo(0);
                    break;
                case SukiyakiStep.MixEgg:
                    railMover.MoveTo(1);
                    break;
                case SukiyakiStep.PickMeat:
                    railMover.MoveTo(2);
                    break;
                case SukiyakiStep.PutInBroth:
                    railMover.MoveTo(3);
                    break;
                case SukiyakiStep.FlipMeat:
                    railMover.MoveTo(3);
                    break;
                case SukiyakiStep.DipEgg:
                    railMover.MoveTo(1);
                    break;
                case SukiyakiStep.DeliverToMouth:
                    railMover.MoveTo(5);
                    break;
            }
        }

        private bool ShouldMoveDemoRail()
        {
            switch (hashiMovementMode)
            {
                case HashiStageMovementMode.DemoWaypointRail:
                case HashiStageMovementMode.Hybrid:
                    return true;
                case HashiStageMovementMode.HardwarePositionOnly:
                    return false;
                case HashiStageMovementMode.Auto:
                    return inputProvider == null || inputProvider.InputMode == HashiInputMode.Demo;
                default:
                    return false;
            }
        }
    }
}
