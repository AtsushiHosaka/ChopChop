using System;
using ChopChop.Food.Meat;
using UnityEngine;

namespace ChopChop.Food.Tearing
{
    public abstract class MeatTearDriverBehaviour : MonoBehaviour
    {
        public event Action<Vector3> PhysicalTorn;

        public virtual void Configure(MeatTearProfile profile)
        {
        }

        public abstract bool RequestTearAt(Vector3 worldPoint, MeatTearReason reason);

        protected void RaisePhysicalTorn(Vector3 worldPoint)
        {
            PhysicalTorn?.Invoke(worldPoint);
        }
    }
}
