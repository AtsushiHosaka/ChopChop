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
            displayedTheta = Mathf.Lerp(displayedTheta, frame.ThetaDegrees, blend);
            displayedParentRotation = Quaternion.Slerp(displayedParentRotation, baseParentRotation * frame.LocalRotation, blend);

            transform.localRotation = displayedParentRotation;
            lowerChopstick.localRotation = lowerBaseRotation;
            upperChopstick.localRotation = upperBaseRotation * Quaternion.AngleAxis(displayedTheta, NormalizedOpeningAxis());
        }

        private Vector3 NormalizedOpeningAxis()
        {
            return openingAxis.sqrMagnitude > 0.0001f ? openingAxis.normalized : Vector3.forward;
        }
    }
}
