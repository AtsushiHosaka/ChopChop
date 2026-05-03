using ChopChop.Food.Meat;
using UnityEngine;

namespace ChopChop.Food.Tearing
{
    [AddComponentMenu("ChopChop/Food/Manual Meat Tear Driver")]
    public sealed class ManualMeatTearDriver : MeatTearDriverBehaviour
    {
        [SerializeField] private bool raisePhysicalEventOnGameplayTear = false;

        public override bool RequestTearAt(Vector3 worldPoint, MeatTearReason reason)
        {
            if (raisePhysicalEventOnGameplayTear)
            {
                RaisePhysicalTorn(worldPoint);
            }

            return false;
        }
    }
}
