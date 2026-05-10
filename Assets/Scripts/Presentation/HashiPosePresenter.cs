using ChopChop.BLE;
using ChopChop.Input;
using UnityEngine;

namespace ChopChop.Presentation
{
    [AddComponentMenu("ChopChop/Presentation/Hashi Pose Presenter")]
    public sealed class HashiPosePresenter : MonoBehaviour
    {
        [SerializeField] private HashiInputProvider inputProvider;
        [SerializeField] private Transform lowerChopstick;
        [SerializeField] private Transform upperChopstick;
        [SerializeField] private Vector3 openingAxis = Vector3.forward;
        [SerializeField] private float smoothing = 12f;
        [SerializeField, Range(0f, 1f)] private float gripPivotFromTip = 0.72f;
        [SerializeField, Range(0f, 1f)] private float wholeRotationSensitivity = 1f;

        private Quaternion baseParentRotation;
        private Quaternion lowerBaseRotation;
        private Quaternion upperBaseRotation;
        private Vector3 lowerBasePosition;
        private Vector3 upperBasePosition;
        private Vector3 lowerBaseScale;
        private Vector3 upperBaseScale;
        private Vector3 lowerGripPivotLocal;
        private Vector3 upperGripPivotLocal;
        private Vector3 lowerBaseGripPivotParent;
        private Vector3 upperBaseGripPivotParent;
        private Vector3 lowerLengthScaleMask = Vector3.forward;
        private Vector3 upperLengthScaleMask = Vector3.forward;
        private Quaternion displayedParentRotation;
        private Quaternion displayedLowerRotation;
        private Quaternion displayedUpperRotation;
        private Vector3 displayAnchorLocal;
        private bool hasDisplayAnchor;
        private Vector3 lastDisplayOffsetWorld;
        private Vector3 lastPositionWithDisplayOffset;
        private bool hasAppliedDisplayOffset;
        private float displayedTheta;
        private Vector3 displayedPositionOffset;

        public void Configure(HashiInputProvider provider, Transform lower, Transform upper, Vector3 openAxis, float smooth)
        {
            inputProvider = provider;
            lowerChopstick = lower;
            upperChopstick = upper;
            openingAxis = openAxis;
            smoothing = smooth;
            CaptureBasePose();
        }

        private void Reset()
        {
            inputProvider = GetComponent<HashiInputProvider>();
            if (transform.childCount > 0)
            {
                lowerChopstick = transform.GetChild(0);
            }

            if (transform.childCount > 1)
            {
                upperChopstick = transform.GetChild(1);
            }
        }

        private void Awake()
        {
            CaptureBasePose();
        }

        private void CaptureBasePose()
        {
            baseParentRotation = transform.localRotation;
            displayedParentRotation = transform.localRotation;
            lowerBaseRotation = lowerChopstick != null ? lowerChopstick.localRotation : Quaternion.identity;
            upperBaseRotation = upperChopstick != null ? upperChopstick.localRotation : Quaternion.identity;
            lowerBasePosition = lowerChopstick != null ? lowerChopstick.localPosition : Vector3.zero;
            upperBasePosition = upperChopstick != null ? upperChopstick.localPosition : Vector3.zero;
            lowerBaseScale = lowerChopstick != null ? lowerChopstick.localScale : Vector3.one;
            upperBaseScale = upperChopstick != null ? upperChopstick.localScale : Vector3.one;
            lowerGripPivotLocal = InferGripPivotLocal(lowerChopstick);
            upperGripPivotLocal = InferGripPivotLocal(upperChopstick);
            lowerBaseGripPivotParent = LocalPivotParentPosition(
                lowerBasePosition,
                lowerBaseRotation,
                lowerBaseScale,
                lowerGripPivotLocal);
            upperBaseGripPivotParent = LocalPivotParentPosition(
                upperBasePosition,
                upperBaseRotation,
                upperBaseScale,
                upperGripPivotLocal);
            lowerLengthScaleMask = InferLengthScaleMask(lowerChopstick);
            upperLengthScaleMask = InferLengthScaleMask(upperChopstick);
            displayedLowerRotation = lowerBaseRotation;
            displayedUpperRotation = upperBaseRotation;
            CaptureDisplayAnchor();
            hasAppliedDisplayOffset = false;
            lastDisplayOffsetWorld = Vector3.zero;
            lastPositionWithDisplayOffset = Vector3.zero;
            displayedPositionOffset = Vector3.zero;
            ApplyParentRotationKeepingAnchor(AlignedBaseParentRotation(), Vector3.zero);
        }

        private void LateUpdate()
        {
            if (inputProvider == null || lowerChopstick == null || upperChopstick == null)
            {
                return;
            }

            var frame = inputProvider.CurrentFrame;
            if (!frame.HasSample)
            {
                return;
            }

            var blend = smoothing <= 0f ? 1f : 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            var positionBlend = smoothing <= 0f ? 1f : 1f - Mathf.Exp(-(smoothing * 0.35f) * Time.deltaTime);

            if (HashiImuMappingRuntime.UseIndependentImuPose
                && HashiImuMappingRuntime.TryGetCurrentIndependentRotations(
                    NormalizedOpeningAxis(),
                    out var upperImuRotation,
                    out var lowerImuRotation))
            {
                displayedParentRotation = Quaternion.Slerp(
                    displayedParentRotation,
                    AlignedParentRotation(frame.LocalRotation),
                    blend);
                displayedPositionOffset = Vector3.Lerp(displayedPositionOffset, HardwarePositionOffsetFor(frame), positionBlend);
                displayedLowerRotation = Quaternion.Slerp(
                    displayedLowerRotation,
                    lowerBaseRotation * lowerImuRotation,
                    blend);
                displayedUpperRotation = Quaternion.Slerp(
                    displayedUpperRotation,
                    upperBaseRotation * upperImuRotation,
                    blend);

                ApplyParentRotationKeepingAnchor(displayedParentRotation, displayedPositionOffset);
                lowerChopstick.localRotation = displayedLowerRotation;
                upperChopstick.localRotation = displayedUpperRotation;
                ApplyChopstickLengthScale();
                ApplyOpeningSeparation();

                return;
            }

            displayedTheta = Mathf.Lerp(displayedTheta, frame.ThetaDegrees, blend);
            displayedParentRotation = Quaternion.Slerp(
                displayedParentRotation,
                AlignedParentRotation(frame.LocalRotation),
                blend);
            displayedLowerRotation = Quaternion.Slerp(displayedLowerRotation, lowerBaseRotation, blend);
            displayedUpperRotation = Quaternion.Slerp(
                displayedUpperRotation,
                upperBaseRotation * Quaternion.AngleAxis(displayedTheta, NormalizedOpeningAxis()),
                blend);
            displayedPositionOffset = Vector3.Lerp(displayedPositionOffset, HardwarePositionOffsetFor(frame), positionBlend);

            ApplyParentRotationKeepingAnchor(displayedParentRotation, displayedPositionOffset);
            lowerChopstick.localRotation = displayedLowerRotation;
            upperChopstick.localRotation = displayedUpperRotation;
            ApplyChopstickLengthScale();
            ApplyFallbackSeparation(displayedTheta);
        }

        private Vector3 NormalizedOpeningAxis()
        {
            return openingAxis.sqrMagnitude > 0.0001f ? openingAxis.normalized : Vector3.forward;
        }

        private Quaternion AlignedBaseParentRotation()
        {
            return baseParentRotation * Quaternion.Euler(HashiImuMappingRuntime.HashiDisplayAlignmentEuler);
        }

        private Quaternion AlignedParentRotation(Quaternion imuRotation)
        {
            var adjustedImuRotation = Quaternion.Slerp(
                Quaternion.identity,
                imuRotation,
                Mathf.Clamp01(wholeRotationSensitivity));
            return adjustedImuRotation * baseParentRotation * Quaternion.Euler(HashiImuMappingRuntime.HashiDisplayAlignmentEuler);
        }

        private void CaptureDisplayAnchor()
        {
            displayAnchorLocal = (lowerBaseGripPivotParent + upperBaseGripPivotParent) * 0.5f;
            hasDisplayAnchor = true;
        }

        private Vector3 InferGripPivotLocal(Transform chopstick)
        {
            var tip = FindTip(chopstick);
            if (chopstick == null || tip == null)
            {
                return Vector3.zero;
            }

            var tipLocal = chopstick.InverseTransformPoint(tip.position);
            if (tipLocal.sqrMagnitude < 0.000001f)
            {
                return Vector3.zero;
            }

            return Vector3.Lerp(tipLocal, -tipLocal, Mathf.Clamp01(gripPivotFromTip));
        }

        private static Vector3 LocalPivotParentPosition(
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Vector3 pivotLocal)
        {
            return localPosition + localRotation * Vector3.Scale(pivotLocal, localScale);
        }

        private static void ApplyLocalPoseAroundPivot(
            Transform target,
            Quaternion localRotation,
            Vector3 pivotLocal,
            Vector3 pivotParentPosition)
        {
            if (target == null)
            {
                return;
            }

            target.localRotation = localRotation;
            target.localPosition = pivotParentPosition - localRotation * Vector3.Scale(pivotLocal, target.localScale);
        }

        private static Transform FindTip(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name.IndexOf("Tip", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return child;
                }

                var descendant = FindTip(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private static Vector3 InferLengthScaleMask(Transform chopstick)
        {
            var tip = FindTip(chopstick);
            if (chopstick == null || tip == null)
            {
                return Vector3.forward;
            }

            var localTip = chopstick.InverseTransformPoint(tip.position);
            var absX = Mathf.Abs(localTip.x);
            var absY = Mathf.Abs(localTip.y);
            var absZ = Mathf.Abs(localTip.z);
            if (absX >= absY && absX >= absZ)
            {
                return Vector3.right;
            }

            if (absY >= absX && absY >= absZ)
            {
                return Vector3.up;
            }

            return Vector3.forward;
        }

        private void ApplyChopstickLengthScale()
        {
            var lengthScale = Mathf.Clamp(HashiImuMappingRuntime.ChopstickVisualLengthScale, 0.25f, 1.25f);
            lowerChopstick.localScale = ScaleAlongMask(lowerBaseScale, lowerLengthScaleMask, lengthScale);
            upperChopstick.localScale = ScaleAlongMask(upperBaseScale, upperLengthScaleMask, lengthScale);
        }

        private static Vector3 ScaleAlongMask(Vector3 baseScale, Vector3 mask, float scale)
        {
            return new Vector3(
                mask.x > 0.5f ? baseScale.x * scale : baseScale.x,
                mask.y > 0.5f ? baseScale.y * scale : baseScale.y,
                mask.z > 0.5f ? baseScale.z * scale : baseScale.z);
        }

        private static Vector3 HardwarePositionOffsetFor(HashiInputFrame frame)
        {
            return HashiImuMappingRuntime.UseHardwarePosition ? frame.LocalPosition : Vector3.zero;
        }

        private void ApplyParentRotationKeepingAnchor(Quaternion targetLocalRotation, Vector3 hardwarePositionOffsetWorld)
        {
            if (hasAppliedDisplayOffset)
            {
                if ((transform.position - lastPositionWithDisplayOffset).sqrMagnitude < 0.0001f)
                {
                    transform.position -= lastDisplayOffsetWorld;
                }

                hasAppliedDisplayOffset = false;
                lastDisplayOffsetWorld = Vector3.zero;
                lastPositionWithDisplayOffset = Vector3.zero;
            }

            if (!hasDisplayAnchor)
            {
                transform.localRotation = targetLocalRotation;
                ApplyDisplayOffset(hardwarePositionOffsetWorld);
                return;
            }

            var anchorWorld = transform.TransformPoint(displayAnchorLocal);
            transform.localRotation = targetLocalRotation;
            transform.position += anchorWorld - transform.TransformPoint(displayAnchorLocal);
            ApplyDisplayOffset(hardwarePositionOffsetWorld);
        }

        private void ApplyDisplayOffset(Vector3 hardwarePositionOffsetWorld)
        {
            lastDisplayOffsetWorld = HashiImuMappingRuntime.HashiDisplayOffsetWorld + hardwarePositionOffsetWorld;
            if (lastDisplayOffsetWorld.sqrMagnitude < 0.000001f)
            {
                return;
            }

            transform.position += lastDisplayOffsetWorld;
            lastPositionWithDisplayOffset = transform.position;
            hasAppliedDisplayOffset = true;
        }

        private void ApplyOpeningSeparation()
        {
            ApplyFallbackSeparation(ResolveOpeningDegrees(inputProvider != null ? inputProvider.CurrentFrame : HashiInputFrame.Empty));
        }

        private static float ResolveOpeningDegrees(HashiInputFrame frame)
        {
            var imu1 = HashiImuMappingRuntime.Imu1PoseDebugState;
            var imu2 = HashiImuMappingRuntime.Imu2PoseDebugState;
            var independentOpening = Mathf.Max(
                Mathf.Abs(imu1.OpeningAngleDegrees),
                Mathf.Abs(imu2.OpeningAngleDegrees));
            return Mathf.Max(Mathf.Abs(frame.ThetaDegrees), independentOpening);
        }

        private void ApplyFallbackSeparation(float openingDegrees)
        {
            var axis = HashiImuMappingRuntime.ChopstickSeparationAxis;
            axis = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.right;
            var separation = Mathf.Min(
                Mathf.Abs(openingDegrees) * Mathf.Max(0f, HashiImuMappingRuntime.ChopstickOpeningSeparationPerDegree),
                Mathf.Max(0f, HashiImuMappingRuntime.MaxChopstickOpeningSeparation));
            var halfOffset = axis * (separation * 0.5f);
            ApplyLocalPoseAroundPivot(
                lowerChopstick,
                displayedLowerRotation,
                lowerGripPivotLocal,
                lowerBaseGripPivotParent - halfOffset);
            ApplyLocalPoseAroundPivot(
                upperChopstick,
                displayedUpperRotation,
                upperGripPivotLocal,
                upperBaseGripPivotParent + halfOffset);
        }
    }
}
