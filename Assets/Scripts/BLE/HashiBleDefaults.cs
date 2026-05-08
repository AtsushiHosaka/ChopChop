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
}
