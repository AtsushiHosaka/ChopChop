using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ChopChop.BLE
{
    public static class HashiBleSampleParser
    {
        private static readonly Regex NumericFieldRegex = new Regex(
            "\"(?<key>[A-Za-z_][A-Za-z0-9_]*)\"\\s*:\\s*(?<value>[-+]?(?:\\d+\\.?\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly char[] LineSeparators = { '\r', '\n' };
        private static readonly char[] CsvSeparators = { ',' };

        public static bool TryParse(string payload, out HashiBleSample sample, out string error)
        {
            sample = HashiBleSample.Create(payload);
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(payload))
            {
                error = "Payload is empty.";
                return false;
            }

            var trimmed = payload.Trim();
            if (trimmed.StartsWith("{"))
            {
                return TryParseJsonObject(trimmed, ref sample, out error);
            }

            return TryParseDualImuCsv(trimmed, ref sample, out error);
        }

        private static bool TryParseJsonObject(string trimmed, ref HashiBleSample sample, out string error)
        {
            error = string.Empty;

            if (!trimmed.EndsWith("}"))
            {
                error = "Payload is not a JSON object.";
                return false;
            }

            var matches = NumericFieldRegex.Matches(trimmed);
            if (matches.Count == 0)
            {
                error = "Payload contains no numeric fields.";
                return false;
            }

            var recognizedFields = 0;
            foreach (Match match in matches)
            {
                var key = match.Groups["key"].Value;
                var valueText = match.Groups["value"].Value;

                if (!double.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    error = $"Field '{key}' is not a valid number.";
                    return false;
                }

                if (ApplyField(ref sample, key, number))
                {
                    recognizedFields++;
                }
            }

            if (recognizedFields == 0)
            {
                error = "Payload did not contain any recognized Hashi fields.";
                return false;
            }

            return true;
        }

        private static bool TryParseDualImuCsv(string trimmed, ref HashiBleSample sample, out string error)
        {
            error = string.Empty;

            var lines = trimmed.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                error = "Payload contains no IMU rows.";
                return false;
            }

            ImuReading imu1 = default;
            ImuReading imu2 = default;
            var parsedRows = 0;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var fields = line.Split(CsvSeparators);
                if (fields.Length != 8)
                {
                    error = "IMU CSV rows must contain name, ax, ay, az, gx, gy, gz, and status.";
                    return false;
                }

                var name = fields[0].Trim();
                if (!IsImuName(name))
                {
                    error = "IMU CSV row must start with IMU1 or IMU2.";
                    return false;
                }

                if (!TryReadFloat(fields[1], "ax", out var ax, out error)
                    || !TryReadFloat(fields[2], "ay", out var ay, out error)
                    || !TryReadFloat(fields[3], "az", out var az, out error)
                    || !TryReadFloat(fields[4], "gx", out var gx, out error)
                    || !TryReadFloat(fields[5], "gy", out var gy, out error)
                    || !TryReadFloat(fields[6], "gz", out var gz, out error)
                    || !TryReadFloat(fields[7], "status", out _, out error))
                {
                    return false;
                }

                var reading = new ImuReading(
                    new Vector3(ax, ay, az),
                    new Vector3(gx, gy, gz));

                if (name.Equals("IMU1", StringComparison.OrdinalIgnoreCase))
                {
                    imu1 = reading;
                }
                else
                {
                    imu2 = reading;
                }

                parsedRows++;
            }

            if (parsedRows == 0)
            {
                error = "Payload contains no IMU rows.";
                return false;
            }

            if (!imu1.HasValue || !imu2.HasValue)
            {
                error = "Dual IMU CSV payload must include both IMU1 and IMU2 rows.";
                return false;
            }

            sample.SetImu1(imu1.AccelerationRaw, imu1.GyroRaw);
            sample.SetImu2(imu2.AccelerationRaw, imu2.GyroRaw);

            if (!HashiImuMappingRuntime.TrySelectMotion(sample, out var acceleration, out var gyro))
            {
                acceleration = imu1.AccelerationG;
                gyro = imu1.GyroDegreesPerSecond;
            }

            sample.SetAccelerationMagnitude(acceleration.magnitude);
            sample.SetAx(acceleration.x);
            sample.SetAy(acceleration.y);
            sample.SetAz(acceleration.z);
            sample.SetGx(gyro.x);
            sample.SetGy(gyro.y);
            sample.SetGz(gyro.z);

            if (!HashiImuMappingRuntime.UseIndependentImuPose
                && TryEstimateOpeningAngle(imu1.AccelerationG, imu2.AccelerationG, out var theta))
            {
                sample.SetTheta(HashiImuMappingRuntime.MapTheta(sample, theta));
            }

            return true;
        }

        private static bool ApplyField(ref HashiBleSample sample, string key, double number)
        {
            switch (key)
            {
                case "theta":
                    sample.SetTheta((float)number);
                    return true;
                case "pinch":
                    sample.SetPinch((float)number);
                    return true;
                case "vel":
                    sample.SetVelocity((float)number);
                    return true;
                case "acc":
                    sample.SetAccelerationMagnitude((float)number);
                    return true;
                case "ax":
                    sample.SetAx((float)number);
                    return true;
                case "ay":
                    sample.SetAy((float)number);
                    return true;
                case "az":
                    sample.SetAz((float)number);
                    return true;
                case "gx":
                    sample.SetGx((float)number);
                    return true;
                case "gy":
                    sample.SetGy((float)number);
                    return true;
                case "gz":
                    sample.SetGz((float)number);
                    return true;
                case "ts":
                    sample.SetTimestamp(number);
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsImuName(string value)
        {
            return value.Equals("IMU1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("IMU2", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryReadFloat(string text, string fieldName, out float value, out string error)
        {
            if (!float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                error = $"Field '{fieldName}' is not a valid number.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static bool TryEstimateOpeningAngle(Vector3 firstAcceleration, Vector3 secondAcceleration, out float theta)
        {
            theta = 0f;

            if (firstAcceleration.sqrMagnitude < 0.000001f || secondAcceleration.sqrMagnitude < 0.000001f)
            {
                return false;
            }

            theta = Vector3.Angle(firstAcceleration, secondAcceleration);
            return !float.IsNaN(theta);
        }

        private readonly struct ImuReading
        {
            public ImuReading(Vector3 accelerationRaw, Vector3 gyroRaw)
            {
                AccelerationRaw = accelerationRaw;
                GyroRaw = gyroRaw;
                HasValue = true;
            }

            public bool HasValue { get; }
            public Vector3 AccelerationRaw { get; }
            public Vector3 GyroRaw { get; }
            public Vector3 AccelerationG => AccelerationRaw * HashiImuMappingRuntime.RawAccelerationToG;
            public Vector3 GyroDegreesPerSecond => GyroRaw * HashiImuMappingRuntime.RawGyroToDegreesPerSecond;
        }
    }
}
