using System;
using ChopChop.Levels.Sukiyaki;
using UnityEngine;

namespace ChopChop.Presentation
{
    [AddComponentMenu("ChopChop/Presentation/Sukiyaki Camera Director")]
    public sealed class SukiyakiCameraDirector : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private SukiyakiStageController stageController;
        [SerializeField] private SukiyakiStageLayout stageLayout;
        [SerializeField] private bool lockAtInitialPose;
        [SerializeField] private float defaultMoveSeconds = 0.8f;
        [SerializeField] private Vector3 overviewFocus = new Vector3(0f, 0.08f, 0f);
        [SerializeField] private StepCameraPose[] stepPoses = Array.Empty<StepCameraPose>();

        private Vector3 startPosition;
        private Quaternion startRotation;
        private float startFov;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private float targetFov;
        private float transitionStartedAt;
        private float transitionSeconds;
        private bool moving;
        private SukiyakiStep lastStep;
        private bool hasLastStep;

        private void Reset()
        {
            targetCamera = GetComponent<Camera>();
            stageController = FindAnyObjectByType<SukiyakiStageController>();
            stageLayout = FindAnyObjectByType<SukiyakiStageLayout>();
            stepPoses = CreateDefaultPoses();
        }

        private void Awake()
        {
            ResolveReferences();
            if (stepPoses == null || stepPoses.Length == 0)
            {
                stepPoses = CreateDefaultPoses();
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (lockAtInitialPose)
            {
                return;
            }

            if (stageController != null)
            {
                stageController.StepChanged += HandleStepChanged;
                lastStep = stageController.CurrentStep;
                hasLastStep = true;
                MoveTo(lastStep, true);
            }
        }

        private void OnDisable()
        {
            if (stageController != null)
            {
                stageController.StepChanged -= HandleStepChanged;
            }
        }

        private void LateUpdate()
        {
            if (lockAtInitialPose)
            {
                return;
            }

            if (stageController != null && (!hasLastStep || stageController.CurrentStep != lastStep))
            {
                lastStep = stageController.CurrentStep;
                hasLastStep = true;
                MoveTo(lastStep, false);
            }

            if (!moving || targetCamera == null)
            {
                return;
            }

            var t = transitionSeconds <= 0f
                ? 1f
                : Mathf.Clamp01((Time.time - transitionStartedAt) / transitionSeconds);
            var smooth = Smooth(t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, smooth);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smooth);
            targetCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, smooth);

            if (t >= 1f)
            {
                moving = false;
            }
        }

        private void HandleStepChanged(SukiyakiStep step)
        {
            lastStep = step;
            hasLastStep = true;
            MoveTo(step, false);
        }

        private void MoveTo(SukiyakiStep step, bool snap)
        {
            if (targetCamera == null || stageLayout == null)
            {
                return;
            }

            var pose = PoseFor(step);
            var focus = FocusFor(step) + pose.LookAtOffset;
            targetPosition = focus + pose.Offset;
            targetRotation = Quaternion.LookRotation(focus - targetPosition, Vector3.up);
            targetFov = pose.FieldOfView;

            if (snap)
            {
                transform.SetPositionAndRotation(targetPosition, targetRotation);
                targetCamera.fieldOfView = targetFov;
                moving = false;
                return;
            }

            startPosition = transform.position;
            startRotation = transform.rotation;
            startFov = targetCamera.fieldOfView;
            transitionStartedAt = Time.time;
            transitionSeconds = pose.MoveSeconds > 0f ? pose.MoveSeconds : defaultMoveSeconds;
            moving = true;
        }

        private Vector3 FocusFor(SukiyakiStep step)
        {
            switch (step)
            {
                case SukiyakiStep.MixEgg:
                case SukiyakiStep.DipEgg:
                    return stageLayout.Egg.Position;
                case SukiyakiStep.PickMeat:
                    return stageLayout.RawMeat.Position;
                case SukiyakiStep.PutInBroth:
                case SukiyakiStep.FlipMeat:
                    return stageLayout.Broth.Position;
                case SukiyakiStep.DeliverToMouth:
                    return stageLayout.Mouth.Position;
                case SukiyakiStep.Calibration:
                    return stageLayout.Calibration.Position;
                case SukiyakiStep.Result:
                default:
                    return overviewFocus;
            }
        }

        private StepCameraPose PoseFor(SukiyakiStep step)
        {
            if (stepPoses != null)
            {
                for (var i = 0; i < stepPoses.Length; i++)
                {
                    if (stepPoses[i].Step == step)
                    {
                        return stepPoses[i];
                    }
                }
            }

            return new StepCameraPose(step, new Vector3(0f, 0.95f, -1.6f), new Vector3(0f, 0.12f, 0f), 42f, defaultMoveSeconds);
        }

        private void ResolveReferences()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (stageController == null)
            {
                stageController = FindAnyObjectByType<SukiyakiStageController>();
            }

            if (stageLayout == null)
            {
                stageLayout = FindAnyObjectByType<SukiyakiStageLayout>();
            }
        }

        private static StepCameraPose[] CreateDefaultPoses()
        {
            return new[]
            {
                new StepCameraPose(SukiyakiStep.Calibration, new Vector3(0.2f, 1.1f, -1.65f), new Vector3(0f, 0.18f, 0f), 46f, 0.55f),
                new StepCameraPose(SukiyakiStep.MixEgg, new Vector3(0.0f, 0.78f, -1.35f), new Vector3(0f, 0.16f, 0f), 40f, 0.85f),
                new StepCameraPose(SukiyakiStep.PickMeat, new Vector3(-0.08f, 0.62f, -0.92f), new Vector3(0f, 0.10f, 0f), 35f, 0.85f),
                new StepCameraPose(SukiyakiStep.PutInBroth, new Vector3(-0.08f, 0.64f, -0.94f), new Vector3(0f, 0.12f, 0f), 35f, 0.85f),
                new StepCameraPose(SukiyakiStep.FlipMeat, new Vector3(-0.22f, 0.66f, -0.92f), new Vector3(0f, 0.12f, 0f), 34f, 0.85f),
                new StepCameraPose(SukiyakiStep.DipEgg, new Vector3(0.08f, 0.78f, -1.35f), new Vector3(0f, 0.16f, 0f), 40f, 0.85f),
                new StepCameraPose(SukiyakiStep.DeliverToMouth, new Vector3(0f, 0.68f, -1.1f), new Vector3(0f, 0.04f, 0f), 38f, 0.9f),
                new StepCameraPose(SukiyakiStep.Result, new Vector3(0f, 1.18f, -2.15f), new Vector3(0f, 0f, 0f), 46f, 0.9f)
            };
        }

        private static float Smooth(float t)
        {
            return t * t * (3f - 2f * t);
        }

        [Serializable]
        public struct StepCameraPose
        {
            [SerializeField] private SukiyakiStep step;
            [SerializeField] private Vector3 offset;
            [SerializeField] private Vector3 lookAtOffset;
            [SerializeField] private float fieldOfView;
            [SerializeField] private float moveSeconds;

            public SukiyakiStep Step => step;
            public Vector3 Offset => offset;
            public Vector3 LookAtOffset => lookAtOffset;
            public float FieldOfView => fieldOfView;
            public float MoveSeconds => moveSeconds;

            public StepCameraPose(SukiyakiStep step, Vector3 offset, Vector3 lookAtOffset, float fieldOfView, float moveSeconds)
            {
                this.step = step;
                this.offset = offset;
                this.lookAtOffset = lookAtOffset;
                this.fieldOfView = fieldOfView;
                this.moveSeconds = moveSeconds;
            }
        }
    }
}
