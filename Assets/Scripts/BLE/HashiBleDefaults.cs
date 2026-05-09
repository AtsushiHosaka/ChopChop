using UnityEngine;

namespace ChopChop.BLE
{
    public static class HashiBleDefaults
    {
        public const string DeviceNameFilter = "Dual_IMU_ESP32";
        public const string ServiceUuid = "12345678-1234-1234-1234-1234567890ab";
        public const string CharacteristicUuid = "abcd1234-5678-90ab-cdef-1234567890ab";

        // The ESP32 sketch configures the LSM6 IMUs with CTRL1/CTRL2 = 0x64.
        public const float RawAccelerationToG = 0.000488f;
        public const float RawGyroToDegreesPerSecond = 0.0175f;
    }

    public enum HashiParentMotionSource
    {
        Lower,
        Upper,
        Average
    }

    [System.Serializable]
    public sealed class HashiImuAxisMapping
    {
        [Tooltip("Raw axis used as logical X after mounting correction.")]
        public HashiImuAxis mapX = HashiImuAxis.X;

        [Tooltip("Raw axis used as logical Y after mounting correction.")]
        public HashiImuAxis mapY = HashiImuAxis.Y;

        [Tooltip("Raw axis used as logical Z after mounting correction.")]
        public HashiImuAxis mapZ = HashiImuAxis.Z;

        [Tooltip("Sign applied while remapping raw axes. Use +/-1 per component.")]
        public Vector3 axisSigns = Vector3.one;

        [Tooltip("Extra sign correction for acceleration only. Use +/-1 per component.")]
        public Vector3 accelerationSigns = Vector3.one;

        [Tooltip("Extra sign correction for gyro only. Use +/-1 per component.")]
        public Vector3 gyroSigns = Vector3.one;

        [Tooltip("Additional basis correction after axis remap. Tune only when the PCB is mounted rotated relative to the chopstick model.")]
        public Vector3 mountingEuler = Vector3.zero;

        public Vector3 MapAccelerationRaw(Vector3 raw)
        {
            var mapped = MapRawAxes(raw);
            mapped = MultiplyComponents(mapped, Signed(accelerationSigns));
            return Quaternion.Euler(mountingEuler) * mapped;
        }

        public Vector3 MapGyroRawToDps(Vector3 raw, float rawGyroToDegreesPerSecond)
        {
            var mapped = MapRawAxes(raw);
            mapped = MultiplyComponents(mapped, Signed(gyroSigns));
            return (Quaternion.Euler(mountingEuler) * mapped) * rawGyroToDegreesPerSecond;
        }

        private Vector3 MapRawAxes(Vector3 raw)
        {
            return new Vector3(
                ReadAxis(raw, mapX) * Sign(axisSigns.x),
                ReadAxis(raw, mapY) * Sign(axisSigns.y),
                ReadAxis(raw, mapZ) * Sign(axisSigns.z));
        }

        public static float ReadAxis(Vector3 value, HashiImuAxis axis)
        {
            switch (axis)
            {
                case HashiImuAxis.X: return value.x;
                case HashiImuAxis.Y: return value.y;
                case HashiImuAxis.Z: return value.z;
                default: return 0f;
            }
        }

        public static Vector3 AxisVector(HashiImuAxis axis)
        {
            switch (axis)
            {
                case HashiImuAxis.X: return Vector3.right;
                case HashiImuAxis.Y: return Vector3.up;
                case HashiImuAxis.Z: return Vector3.forward;
                default: return Vector3.right;
            }
        }

        private static Vector3 MultiplyComponents(Vector3 value, Vector3 multiplier)
        {
            return new Vector3(value.x * multiplier.x, value.y * multiplier.y, value.z * multiplier.z);
        }

        private static Vector3 Signed(Vector3 value)
        {
            return new Vector3(Sign(value.x), Sign(value.y), Sign(value.z));
        }

        private static float Sign(float value)
        {
            return value < 0f ? -1f : 1f;
        }
    }
}
