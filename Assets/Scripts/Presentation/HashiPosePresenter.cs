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

        private Quaternion baseParentRotation;
        private Quaternion lowerBaseRotation;
        private Quaternion upperBaseRotation;
        private Vector3 lowerBasePosition;
        private Vector3 upperBasePosition;
        private Vector3 lowerBaseScale;
        private Vector3 upperBaseScale;
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
        private float lastAcceptedBleRealtime = -1f;
        private float ignoreBleUntilRealtime;
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
            lowerLengthScaleMask = InferLengthScaleMask(lowerChopstick);
            upperLengthScaleMask = InferLengthScaleMask(upperChopstick);
            displayedLowerRotation = lowerBaseRotation;
            displayedUpperRotation = upperBaseRotation;
            CaptureDisplayAnchor();
            hasAppliedDisplayOffset = false;
            lastDisplayOffsetWorld = Vector3.zero;
            lastPositionWithDisplayOffset = Vector3.zero;
            lastAcceptedBleRealtime = -1f;
            ignoreBleUntilRealtime = 0f;
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

            if (HashiImuMappingRuntime.UseIndependentImuPose)
            {
                if (inputProvider.TryGetLatestBleSample(out var sample)
                    && sample.HasDualImu
                    && ShouldUseBleSample(sample)
                    && HashiImuMappingRuntime.UpdateIndependentRotations(
                        sample,
                        Time.deltaTime,
                        NormalizedOpeningAxis(),
                        out var upperImuRotation,
                        out var lowerImuRotation))
                {
                    displayedParentRotation = Quaternion.Slerp(
                        displayedParentRotation,
                        AlignedBaseParentRotation() * frame.LocalRotation,
                        blend);
                    displayedLowerRotation = Quaternion.Slerp(displayedLowerRotation, lowerBaseRotation * lowerImuRotation, blend);
                    displayedUpperRotation = Quaternion.Slerp(displayedUpperRotation, upperBaseRotation * upperImuRotation, blend);
                    displayedPositionOffset = Vector3.Lerp(displayedPositionOffset, HardwarePositionOffsetFor(frame), blend);

                    ApplyParentRotationKeepingAnchor(displayedParentRotation, displayedPositionOffset);
                    lowerChopstick.localRotation = displayedLowerRotation;
                    upperChopstick.localRotation = displayedUpperRotation;
                    ApplyChopstickLengthScale();
                    ApplyOpeningSeparation();
                    AcceptBleSample(sample);
                }

                return;
            }

            displayedTheta = Mathf.Lerp(displayedTheta, frame.ThetaDegrees, blend);
            displayedParentRotation = Quaternion.Slerp(
                displayedParentRotation,
                AlignedBaseParentRotation() * frame.LocalRotation,
                blend);
            displayedLowerRotation = Quaternion.Slerp(displayedLowerRotation, lowerBaseRotation, blend);
            displayedUpperRotation = Quaternion.Slerp(
                displayedUpperRotation,
                upperBaseRotation * Quaternion.AngleAxis(displayedTheta, NormalizedOpeningAxis()),
                blend);
            displayedPositionOffset = Vector3.Lerp(displayedPositionOffset, HardwarePositionOffsetFor(frame), blend);

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

        private void CaptureDisplayAnchor()
        {
            hasDisplayAnchor = false;
            displayAnchorLocal = Vector3.zero;

            var count = 0;
            AddAnchorPoint(lowerChopstick, ref count);
            AddAnchorPoint(upperChopstick, ref count);
            AddAnchorPoint(FindTip(lowerChopstick), ref count);
            AddAnchorPoint(FindTip(upperChopstick), ref count);

            if (count <= 0)
            {
                return;
            }

            displayAnchorLocal /= count;
            hasDisplayAnchor = true;
        }

        private void AddAnchorPoint(Transform point, ref int count)
        {
            if (point == null)
            {
                return;
            }

            displayAnchorLocal += transform.InverseTransformPoint(point.position);
            count++;
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

        private bool ShouldUseBleSample(HashiBleSample sample)
        {
            var now = Time.realtimeSinceStartup;
            var maxAge = Mathf.Max(0f, HashiImuMappingRuntime.MaxBleSampleAgeSeconds);
            if (maxAge > 0f && now - sample.RealtimeReceived > maxAge)
            {
                return false;
            }

            if (now < ignoreBleUntilRealtime)
            {
                return false;
            }

            var maxGap = Mathf.Max(0f, HashiImuMappingRuntime.MaxBleSampleAgeSeconds);
            if (lastAcceptedBleRealtime > 0f && maxGap > 0f && sample.RealtimeReceived - lastAcceptedBleRealtime > maxGap)
            {
                ignoreBleUntilRealtime = now + Mathf.Max(0f, HashiImuMappingRuntime.BleResumeSettleSeconds);
                lastAcceptedBleRealtime = sample.RealtimeReceived;
                return false;
            }

            return true;
        }

        private void AcceptBleSample(HashiBleSample sample)
        {
            lastAcceptedBleRealtime = sample.RealtimeReceived;
        }

        private void ApplyOpeningSeparation()
        {
            var imu1 = HashiImuMappingRuntime.Imu1PoseDebugState;
            var imu2 = HashiImuMappingRuntime.Imu2PoseDebugState;
            ApplyFallbackSeparation(Mathf.Max(Mathf.Abs(imu1.OpeningAngleDegrees), Mathf.Abs(imu2.OpeningAngleDegrees)));
        }

        private void ApplyFallbackSeparation(float openingDegrees)
        {
            var axis = HashiImuMappingRuntime.ChopstickSeparationAxis;
            axis = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.right;
            var separation = Mathf.Min(
                Mathf.Abs(openingDegrees) * Mathf.Max(0f, HashiImuMappingRuntime.ChopstickOpeningSeparationPerDegree),
                Mathf.Max(0f, HashiImuMappingRuntime.MaxChopstickOpeningSeparation));
            var halfOffset = axis * (separation * 0.5f);
            lowerChopstick.localPosition = lowerBasePosition - halfOffset;
            upperChopstick.localPosition = upperBasePosition + halfOffset;
        }
    }
}
