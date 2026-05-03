using System;
using UnityEngine;

namespace ChopChop.Food.Meat
{
    [Serializable]
    public struct MeatStressFrame
    {
        [SerializeField] private float poke;
        [SerializeField] private float shear;
        [SerializeField] private float twist;
        [SerializeField] private float tension;
        [SerializeField] private float compression;
        [SerializeField] private float damagePerSecond;
        [SerializeField] private MeatTearReason dominantReason;
        [SerializeField] private Vector3 worldContactPoint;
        [SerializeField] private bool hasContact;

        public float Poke => poke;
        public float Shear => shear;
        public float Twist => twist;
        public float Tension => tension;
        public float Compression => compression;
        public float DamagePerSecond => damagePerSecond;
        public MeatTearReason DominantReason => dominantReason;
        public Vector3 WorldContactPoint => worldContactPoint;
        public bool HasContact => hasContact;

        public MeatStressFrame(
            float poke,
            float shear,
            float twist,
            float tension,
            float compression,
            float damagePerSecond,
            MeatTearReason dominantReason,
            Vector3 worldContactPoint,
            bool hasContact)
        {
            this.poke = poke;
            this.shear = shear;
            this.twist = twist;
            this.tension = tension;
            this.compression = compression;
            this.damagePerSecond = damagePerSecond;
            this.dominantReason = dominantReason;
            this.worldContactPoint = worldContactPoint;
            this.hasContact = hasContact;
        }

        public static MeatStressFrame None => new MeatStressFrame(
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            MeatTearReason.Unknown,
            Vector3.zero,
            false);
    }
}
