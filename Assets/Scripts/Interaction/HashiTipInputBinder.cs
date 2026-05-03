using ChopChop.Input;
using UnityEngine;

namespace ChopChop.Interaction
{
    [AddComponentMenu("ChopChop/Interaction/Hashi Tip Input Binder")]
    public sealed class HashiTipInputBinder : MonoBehaviour
    {
        [SerializeField] private HashiInputProvider inputProvider;
        [SerializeField] private ChopstickTip lowerTip;
        [SerializeField] private ChopstickTip upperTip;

        public void Configure(HashiInputProvider provider, ChopstickTip lower, ChopstickTip upper)
        {
            inputProvider = provider;
            lowerTip = lower;
            upperTip = upper;
        }

        private void Reset()
        {
            inputProvider = GetComponentInParent<HashiInputProvider>();
            var tips = GetComponentsInChildren<ChopstickTip>();
            if (tips.Length > 0)
            {
                lowerTip = tips[0];
            }

            if (tips.Length > 1)
            {
                upperTip = tips[1];
            }
        }

        private void LateUpdate()
        {
            if (inputProvider == null)
            {
                return;
            }

            var frame = inputProvider.CurrentFrame;
            if (!frame.HasSample)
            {
                return;
            }

            lowerTip?.SetInputHints(frame.PinchNormalized, frame.ThetaDegrees);
            upperTip?.SetInputHints(frame.PinchNormalized, frame.ThetaDegrees);
        }
    }
}
