using System;
using System.Diagnostics;
using UnityEngine;

namespace ChopChop.BLE
{
    public struct HashiBlePayload
    {
        public string Text;
        public double HostTimeSeconds;

        public HashiBlePayload(string text, double hostTimeSeconds)
        {
            Text = text ?? string.Empty;
            HostTimeSeconds = hostTimeSeconds;
        }
    }

    public static class HashiBleClock
    {
        public static double NowSeconds
        {
            get { return Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency; }
        }
    }

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
        [SerializeField] private bool hasImu1Acceleration;
        [SerializeField] private bool hasImu1Gyro;
        [SerializeField] private Vector3 imu1AccelerationRaw;
        [SerializeField] private Vector3 imu1GyroRaw;
        [SerializeField] private bool hasImu2Acceleration;
        [SerializeField] private bool hasImu2Gyro;
        [SerializeField] private Vector3 imu2AccelerationRaw;
        [SerializeField] private Vector3 imu2GyroRaw;
        [SerializeField] private bool hasSequence;
        [SerializeField] private int sequence;
        [SerializeField] private bool hasDeviceTime;
        [SerializeField] private double deviceTimeSeconds;
        [SerializeField] private double hostTimeSeconds;
        [SerializeField] private float realtimeReceived;

        private static int nextSequence;

        public string RawJson { get; private set; }
        public float RealtimeReceived => realtimeReceived;
        public int Sequence => sequence;

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
        public bool HasSequence => hasSequence;
        public bool HasDeviceTime => hasDeviceTime;
        public bool HasImu1Acceleration => hasImu1Acceleration;
        public bool HasImu1Gyro => hasImu1Gyro;
        public bool HasImu2Acceleration => hasImu2Acceleration;
        public bool HasImu2Gyro => hasImu2Gyro;
        public bool HasImu1 => hasImu1Acceleration && hasImu1Gyro;
        public bool HasImu2 => hasImu2Acceleration && hasImu2Gyro;
        public bool HasDualImu => HasImu1 && HasImu2;

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
        public double HostTimeSeconds => hostTimeSeconds;
        public double DeviceTimeSeconds => deviceTimeSeconds;
        public double BestTimeSeconds => hasDeviceTime ? deviceTimeSeconds : hostTimeSeconds;

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

        internal static HashiBleSample Create(string rawPayload)
        {
            return Create(new HashiBlePayload(rawPayload, HashiBleClock.NowSeconds));
        }

        internal static HashiBleSample Create(HashiBlePayload payload)
        {
            var hostNow = HashiBleClock.NowSeconds;
            var unityNow = Time.realtimeSinceStartupAsDouble;
            var receivedRealtime = unityNow;
            if (payload.HostTimeSeconds > 0.0 && hostNow >= payload.HostTimeSeconds)
            {
                receivedRealtime = unityNow - (hostNow - payload.HostTimeSeconds);
            }

            return new HashiBleSample
            {
                RawJson = payload.Text ?? string.Empty,
                hostTimeSeconds = payload.HostTimeSeconds > 0.0 ? payload.HostTimeSeconds : hostNow,
                realtimeReceived = (float)Math.Max(0.0, receivedRealtime),
                sequence = unchecked(++nextSequence),
                hasSequence = false
            };
        }

        internal void SetSequence(int value)
        {
            sequence = value;
            hasSequence = true;
        }

        internal void SetDeviceTimeSeconds(double value)
        {
            deviceTimeSeconds = value;
            hasDeviceTime = true;
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
            SetImu1Acceleration(accelerationRaw);
            SetImu1Gyro(gyroRaw);
        }

        internal void SetImu2(Vector3 accelerationRaw, Vector3 gyroRaw)
        {
            SetImu2Acceleration(accelerationRaw);
            SetImu2Gyro(gyroRaw);
        }

        internal void SetImu1Acceleration(Vector3 accelerationRaw)
        {
            imu1AccelerationRaw = accelerationRaw;
            hasImu1Acceleration = true;
        }

        internal void SetImu1Gyro(Vector3 gyroRaw)
        {
            imu1GyroRaw = gyroRaw;
            hasImu1Gyro = true;
        }

        internal void SetImu2Acceleration(Vector3 accelerationRaw)
        {
            imu2AccelerationRaw = accelerationRaw;
            hasImu2Acceleration = true;
        }

        internal void SetImu2Gyro(Vector3 gyroRaw)
        {
            imu2GyroRaw = gyroRaw;
            hasImu2Gyro = true;
        }
    }
}
