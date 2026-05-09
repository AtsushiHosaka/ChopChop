using System;
using UnityEngine;

namespace ChopChop.BLE
{
    [Serializable]
    public struct HashiBleSample
    {
        [SerializeField] private bool hasTheta;
        [SerializeField] private float theta;
        [SerializeField] private bool hasPinch;
        [SerializeField] private float pinch;
        [SerializeField] private bool hasVelocity;
        [SerializeField] private float velocity;
        [SerializeField] private bool hasAccelerationMagnitude;
        [SerializeField] private float accelerationMagnitude;
        [SerializeField] private bool hasAx;
        [SerializeField] private float ax;
        [SerializeField] private bool hasAy;
        [SerializeField] private float ay;
        [SerializeField] private bool hasAz;
        [SerializeField] private float az;
        [SerializeField] private bool hasGx;
        [SerializeField] private float gx;
        [SerializeField] private bool hasGy;
        [SerializeField] private float gy;
        [SerializeField] private bool hasGz;
        [SerializeField] private float gz;
        [SerializeField] private bool hasTimestamp;
        [SerializeField] private double timestamp;
        [SerializeField] private bool hasImu1;
        [SerializeField] private Vector3 imu1AccelerationRaw;
        [SerializeField] private Vector3 imu1GyroRaw;
        [SerializeField] private bool hasImu2;
        [SerializeField] private Vector3 imu2AccelerationRaw;
        [SerializeField] private Vector3 imu2GyroRaw;

        private static int nextSequence;

        public string RawJson { get; private set; }
        public float RealtimeReceived { get; private set; }
        public int Sequence { get; private set; }

        public bool HasTheta => hasTheta;
        public bool HasPinch => hasPinch;
        public bool HasVelocity => hasVelocity;
        public bool HasAccelerationMagnitude => hasAccelerationMagnitude;
        public bool HasAx => hasAx;
        public bool HasAy => hasAy;
        public bool HasAz => hasAz;
        public bool HasGx => hasGx;
        public bool HasGy => hasGy;
        public bool HasGz => hasGz;
        public bool HasTimestamp => hasTimestamp;
        public bool HasImu1 => hasImu1;
        public bool HasImu2 => hasImu2;
        public bool HasDualImu => hasImu1 && hasImu2;

        public float ThetaValue => theta;
        public float PinchValue => pinch;
        public float VelocityValue => velocity;
        public float AccelerationMagnitudeValue => accelerationMagnitude;
        public float AxValue => ax;
        public float AyValue => ay;
        public float AzValue => az;
        public float GxValue => gx;
        public float GyValue => gy;
        public float GzValue => gz;
        public double TimestampValue => timestamp;

        public float? Theta => hasTheta ? theta : (float?)null;
        public float? Pinch => hasPinch ? pinch : (float?)null;
        public float? Velocity => hasVelocity ? velocity : (float?)null;
        public float? AccelerationMagnitude => hasAccelerationMagnitude ? accelerationMagnitude : (float?)null;
        public float? Ax => hasAx ? ax : (float?)null;
        public float? Ay => hasAy ? ay : (float?)null;
        public float? Az => hasAz ? az : (float?)null;
        public float? Gx => hasGx ? gx : (float?)null;
        public float? Gy => hasGy ? gy : (float?)null;
        public float? Gz => hasGz ? gz : (float?)null;
        public double? Timestamp => hasTimestamp ? timestamp : (double?)null;

        public bool HasAccelerationVector => hasAx && hasAy && hasAz;
        public bool HasGyroVector => hasGx && hasGy && hasGz;
        public Vector3 AccelerationVector => new Vector3(ax, ay, az);
        public Vector3 GyroVector => new Vector3(gx, gy, gz);
        public Vector3 Imu1AccelerationRaw => imu1AccelerationRaw;
        public Vector3 Imu1GyroRaw => imu1GyroRaw;
        public Vector3 Imu2AccelerationRaw => imu2AccelerationRaw;
        public Vector3 Imu2GyroRaw => imu2GyroRaw;
        public Vector3 Imu1AccelerationG => imu1AccelerationRaw * HashiImuMappingRuntime.RawAccelerationToG;
        public Vector3 Imu1GyroDegreesPerSecond => imu1GyroRaw * HashiImuMappingRuntime.RawGyroToDegreesPerSecond;
        public Vector3 Imu2AccelerationG => imu2AccelerationRaw * HashiImuMappingRuntime.RawAccelerationToG;
        public Vector3 Imu2GyroDegreesPerSecond => imu2GyroRaw * HashiImuMappingRuntime.RawGyroToDegreesPerSecond;

        internal static HashiBleSample Create(string rawJson)
        {
            return new HashiBleSample
            {
                RawJson = rawJson,
                RealtimeReceived = Time.realtimeSinceStartup,
                Sequence = unchecked(++nextSequence)
            };
        }

        internal void SetTheta(float value)
        {
            theta = value;
            hasTheta = true;
        }

        internal void SetPinch(float value)
        {
            pinch = value;
            hasPinch = true;
        }

        internal void SetVelocity(float value)
        {
            velocity = value;
            hasVelocity = true;
        }

        internal void SetAccelerationMagnitude(float value)
        {
            accelerationMagnitude = value;
            hasAccelerationMagnitude = true;
        }

        internal void SetAx(float value)
        {
            ax = value;
            hasAx = true;
        }

        internal void SetAy(float value)
        {
            ay = value;
            hasAy = true;
        }

        internal void SetAz(float value)
        {
            az = value;
            hasAz = true;
        }

        internal void SetGx(float value)
        {
            gx = value;
            hasGx = true;
        }

        internal void SetGy(float value)
        {
            gy = value;
            hasGy = true;
        }

        internal void SetGz(float value)
        {
            gz = value;
            hasGz = true;
        }

        internal void SetTimestamp(double value)
        {
            timestamp = value;
            hasTimestamp = true;
        }

        internal void SetImu1(Vector3 accelerationRaw, Vector3 gyroRaw)
        {
            imu1AccelerationRaw = accelerationRaw;
            imu1GyroRaw = gyroRaw;
            hasImu1 = true;
        }

        internal void SetImu2(Vector3 accelerationRaw, Vector3 gyroRaw)
        {
            imu2AccelerationRaw = accelerationRaw;
            imu2GyroRaw = gyroRaw;
            hasImu2 = true;
        }
    }
}
