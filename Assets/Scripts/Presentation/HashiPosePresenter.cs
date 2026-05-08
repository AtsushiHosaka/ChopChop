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
        private Quaternion displayedParentRotation;
        private Quaternion displayedLowerRotation;
        private Quaternion displayedUpperRotation;
        private float displayedTheta;

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
            displayedLowerRotation = lowerBaseRotation;
            displayedUpperRotation = upperBaseRotation;
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

            if (HashiImuMappingRuntime.UseIndependentImuPose
                && inputProvider.TryGetLatestBleSample(out var sample)
                && sample.HasDualImu)
            {
                if (HashiImuMappingRuntime.TryGetIndependentRotations(sample, out var upperImuRotation, out var lowerImuRotation))
                {
                    displayedParentRotation = Quaternion.Slerp(
                        displayedParentRotation,
                        baseParentRotation * frame.LocalRotation,
                        blend);
                    displayedLowerRotation = Quaternion.Slerp(displayedLowerRotation, lowerBaseRotation * lowerImuRotation, blend);
                    displayedUpperRotation = Quaternion.Slerp(displayedUpperRotation, upperBaseRotation * upperImuRotation, blend);

                    transform.localRotation = displayedParentRotation;
                    lowerChopstick.localRotation = displayedLowerRotation;
                    upperChopstick.localRotation = displayedUpperRotation;
                }

                return;
            }

            displayedTheta = Mathf.Lerp(displayedTheta, frame.ThetaDegrees, blend);
            displayedParentRotation = Quaternion.Slerp(
                displayedParentRotation,
                baseParentRotation * frame.LocalRotation,
                blend);
            displayedLowerRotation = Quaternion.Slerp(displayedLowerRotation, lowerBaseRotation, blend);
            displayedUpperRotation = Quaternion.Slerp(
                displayedUpperRotation,
                upperBaseRotation * Quaternion.AngleAxis(displayedTheta, NormalizedOpeningAxis()),
                blend);

            transform.localRotation = displayedParentRotation;
            lowerChopstick.localRotation = displayedLowerRotation;
            upperChopstick.localRotation = displayedUpperRotation;
        }

        private Vector3 NormalizedOpeningAxis()
        {
            return openingAxis.sqrMagnitude > 0.0001f ? openingAxis.normalized : Vector3.forward;
        }
    }
}
