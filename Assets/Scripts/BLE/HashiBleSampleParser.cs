using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ChopChop.BLE
{
    public static class HashiBleSampleParser
    {
        private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
        private const string NumberPattern = @"[-+]?(?:(?:\d+\.?\d*)|(?:\.\d+))(?:[eE][-+]?\d+)?";
        private static readonly Regex NumberRegex = new Regex(NumberPattern, Options | RegexOptions.Compiled);
        private static readonly Regex NumericFieldRegex = new Regex(
            "\"(?<key>[A-Za-z_][A-Za-z0-9_]*)\"\\s*:\\s*(?<value>" + NumberPattern + ")",
            Options | RegexOptions.Compiled);
        private static readonly char[] LineSeparators = { '\r', '\n' };
        private static readonly char[] CsvSeparators = { ',' };

        public static bool TryParse(string payload, out HashiBleSample sample, out string error)
        {
            return TryParse(new HashiBlePayload(payload, HashiBleClock.NowSeconds), out sample, out error);
        }

        public static bool TryParse(HashiBlePayload payload, out HashiBleSample sample, out string error)
        {
            sample = HashiBleSample.Create(payload);
            error = string.Empty;

            var text = payload.Text == null ? string.Empty : payload.Text.Trim();
            if (text.Length == 0)
            {
                error = "Payload is empty.";
                return false;
            }

            if (text.StartsWith("{", StringComparison.Ordinal))
            {
                return TryParseJsonObject(text, ref sample, out error);
            }

            if (TryParseFlatDualImuCsv(text, ref sample))
            {
                PopulateSelectedMotion(ref sample);
                PopulateAccelerationOpeningIfNeeded(ref sample);
                return true;
            }

            return TryParseDualImuRows(text, ref sample, out error);
        }

        private static bool TryParseJsonObject(string text, ref HashiBleSample sample, out string error)
        {
            error = string.Empty;
            if (!text.EndsWith("}", StringComparison.Ordinal))
            {
                error = "Payload is not a JSON object.";
                return false;
            }

            var recognizedFields = 0;
            var matches = NumericFieldRegex.Matches(text);
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

            var hasAnyImu = false;
            if (TryReadImu(text, "imu1", "1", out var imu1Acc, out var imu1Gyro, out var hasImu1Acc, out var hasImu1Gyro))
            {
                if (hasImu1Acc)
                {
                    sample.SetImu1Acceleration(imu1Acc);
                }

                if (hasImu1Gyro)
                {
                    sample.SetImu1Gyro(imu1Gyro);
                }

                hasAnyImu = true;
            }

            if (TryReadImu(text, "imu2", "2", out var imu2Acc, out var imu2Gyro, out var hasImu2Acc, out var hasImu2Gyro))
            {
                if (hasImu2Acc)
                {
                    sample.SetImu2Acceleration(imu2Acc);
                }

                if (hasImu2Gyro)
                {
                    sample.SetImu2Gyro(imu2Gyro);
                }

                hasAnyImu = true;
            }

            if (sample.HasDualImu)
            {
                PopulateSelectedMotion(ref sample);
                PopulateAccelerationOpeningIfNeeded(ref sample);
            }
            else if (sample.HasImu1 || sample.HasImu2)
            {
                PopulateSelectedMotion(ref sample);
            }
            else if (hasAnyImu)
            {
                error = string.Format(
                    CultureInfo.InvariantCulture,
                    "payload must contain usable acc+gyro for at least one IMU. imu1 acc={0} gyro={1}, imu2 acc={2} gyro={3}",
                    sample.HasImu1Acceleration,
                    sample.HasImu1Gyro,
                    sample.HasImu2Acceleration,
                    sample.HasImu2Gyro);
                return false;
            }

            if (recognizedFields == 0 && !sample.HasImu1 && !sample.HasImu2)
            {
                error = "Payload did not contain any recognized Hashi fields.";
                return false;
            }

            return true;
        }

        private static bool TryParseFlatDualImuCsv(string text, ref HashiBleSample sample)
        {
            if (text.IndexOf('{') >= 0 || text.IndexOf('[') >= 0 || text.IndexOf("IMU", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            var matches = NumberRegex.Matches(text);
            if (matches.Count < 12)
            {
                return false;
            }

            var values = new List<float>(matches.Count);
            for (var i = 0; i < matches.Count; i++)
            {
                if (float.TryParse(matches[i].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    values.Add(value);
                }
            }

            if (values.Count < 12)
            {
                return false;
            }

            var start = 0;
            if (values.Count == 13)
            {
                sample.SetSequence(Mathf.RoundToInt(values[0]));
                start = 1;
            }
            else if (values.Count >= 14)
            {
                sample.SetDeviceTimeSeconds(values[0] > 10000f ? values[0] / 1000.0 : values[0]);
                sample.SetSequence(Mathf.RoundToInt(values[1]));
                start = 2;
            }

            if (values.Count - start < 12)
            {
                return false;
            }

            sample.SetImu1(
                new Vector3(values[start], values[start + 1], values[start + 2]),
                new Vector3(values[start + 3], values[start + 4], values[start + 5]));
            sample.SetImu2(
                new Vector3(values[start + 6], values[start + 7], values[start + 8]),
                new Vector3(values[start + 9], values[start + 10], values[start + 11]));
            return true;
        }

        private static bool TryParseDualImuRows(string text, ref HashiBleSample sample, out string error)
        {
            error = string.Empty;

            var lines = text.Split(LineSeparators, StringSplitOptions.RemoveEmptyEntries);
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
            PopulateSelectedMotion(ref sample);
            PopulateAccelerationOpeningIfNeeded(ref sample);
            return true;
        }

        private static bool ApplyField(ref HashiBleSample sample, string key, double number)
        {
            switch (key)
            {
                case "theta":
                case "angle":
                case "opening":
                    sample.SetTheta((float)number);
                    return true;
                case "pinch":
                case "pressure":
                case "force":
                    sample.SetPinch((float)number);
                    return true;
                case "vel":
                case "velocity":
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
                case "seq":
                case "sequence":
                case "n":
                    sample.SetSequence(Mathf.RoundToInt((float)number));
                    return true;
                case "ts":
                    sample.SetTimestamp(number);
                    return true;
                case "time_us":
                case "timestamp_us":
                case "micros":
                case "us":
                    sample.SetDeviceTimeSeconds(number / 1000000.0);
                    return true;
                case "time_ms":
                case "timestamp_ms":
                case "millis":
                case "ms":
                    sample.SetDeviceTimeSeconds(number / 1000.0);
                    return true;
                case "time_s":
                case "timestamp_s":
                case "seconds":
                    sample.SetDeviceTimeSeconds(number);
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryReadImu(
            string text,
            string objectKey,
            string suffix,
            out Vector3 acceleration,
            out Vector3 gyro,
            out bool hasAcceleration,
            out bool hasGyro)
        {
            acceleration = Vector3.zero;
            gyro = Vector3.zero;
            hasAcceleration = false;
            hasGyro = false;

            if (TryExtractObject(text, objectKey, out var imuObject))
            {
                hasAcceleration = TryReadVector(
                    imuObject,
                    out acceleration,
                    "a", "acc", "accel", "acceleration", "xl",
                    "linearAcceleration", "linear_acceleration");
                hasGyro = TryReadVector(
                    imuObject,
                    out gyro,
                    "g", "gyro", "gyr", "angular", "angularRate", "angular_rate");

                if (!hasAcceleration)
                {
                    hasAcceleration = TryReadFlatVector(imuObject, out acceleration,
                        new[] { "ax", "ay", "az" },
                        new[] { "accx", "accy", "accz" },
                        new[] { "xla", "yla", "zla" });
                }

                if (!hasGyro)
                {
                    hasGyro = TryReadFlatVector(imuObject, out gyro,
                        new[] { "gx", "gy", "gz" },
                        new[] { "gyrox", "gyroy", "gyroz" });
                }
            }

            if (!hasAcceleration)
            {
                hasAcceleration = TryReadVector(text, out acceleration,
                    "a" + suffix,
                    "acc" + suffix,
                    "accel" + suffix,
                    "acceleration" + suffix,
                    "imu" + suffix + "_acc",
                    "imu" + suffix + "Acc",
                    "imu" + suffix + "_accel",
                    "imu" + suffix + "Acceleration");
            }

            if (!hasGyro)
            {
                hasGyro = TryReadVector(text, out gyro,
                    "g" + suffix,
                    "gyro" + suffix,
                    "gyr" + suffix,
                    "angular" + suffix,
                    "imu" + suffix + "_gyro",
                    "imu" + suffix + "Gyro",
                    "imu" + suffix + "_gyr");
            }

            if (!hasAcceleration)
            {
                hasAcceleration = TryReadFlatVector(text, out acceleration,
                    new[] { "ax" + suffix, "ay" + suffix, "az" + suffix },
                    new[] { "a" + suffix + "x", "a" + suffix + "y", "a" + suffix + "z" },
                    new[] { "a" + suffix + "_x", "a" + suffix + "_y", "a" + suffix + "_z" },
                    new[] { "imu" + suffix + "_ax", "imu" + suffix + "_ay", "imu" + suffix + "_az" },
                    new[] { "imu" + suffix + "_acc_x", "imu" + suffix + "_acc_y", "imu" + suffix + "_acc_z" });
            }

            if (!hasGyro)
            {
                hasGyro = TryReadFlatVector(text, out gyro,
                    new[] { "gx" + suffix, "gy" + suffix, "gz" + suffix },
                    new[] { "g" + suffix + "x", "g" + suffix + "y", "g" + suffix + "z" },
                    new[] { "g" + suffix + "_x", "g" + suffix + "_y", "g" + suffix + "_z" },
                    new[] { "imu" + suffix + "_gx", "imu" + suffix + "_gy", "imu" + suffix + "_gz" },
                    new[] { "imu" + suffix + "_gyro_x", "imu" + suffix + "_gyro_y", "imu" + suffix + "_gyro_z" });
            }

            return hasAcceleration || hasGyro;
        }

        private static void PopulateSelectedMotion(ref HashiBleSample sample)
        {
            if (!HashiImuMappingRuntime.TrySelectMotion(sample, out var acceleration, out var gyro))
            {
                acceleration = sample.Imu1AccelerationG;
                gyro = sample.Imu1GyroDegreesPerSecond;
            }

            sample.SetAccelerationMagnitude(acceleration.magnitude);
            sample.SetAx(acceleration.x);
            sample.SetAy(acceleration.y);
            sample.SetAz(acceleration.z);
            sample.SetGx(gyro.x);
            sample.SetGy(gyro.y);
            sample.SetGz(gyro.z);
        }

        private static void PopulateAccelerationOpeningIfNeeded(ref HashiBleSample sample)
        {
            if (sample.HasTheta)
            {
                return;
            }

            if (TryEstimateOpeningAngle(sample.Imu1AccelerationG, sample.Imu2AccelerationG, out var theta))
            {
                sample.SetTheta(HashiImuMappingRuntime.MapTheta(sample, theta));
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

        private static bool TryReadVector(string text, out Vector3 vector, params string[] keys)
        {
            vector = Vector3.zero;
            for (var i = 0; i < keys.Length; i++)
            {
                if (TryReadArrayVector(text, keys[i], out vector))
                {
                    return true;
                }

                if (TryReadObjectVector(text, keys[i], out vector))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadArrayVector(string text, string key, out Vector3 vector)
        {
            vector = Vector3.zero;
            var pattern = QuoteKey(key) + @"\s*:\s*\[\s*(" + NumberPattern + @")\s*,\s*(" + NumberPattern + @")\s*,\s*(" + NumberPattern + @")";
            var match = Regex.Match(text, pattern, Options);
            if (!match.Success)
            {
                return false;
            }

            return TryBuildVector(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, out vector);
        }

        private static bool TryReadObjectVector(string text, string key, out Vector3 vector)
        {
            vector = Vector3.zero;
            if (!TryExtractObject(text, key, out var objectText))
            {
                return false;
            }

            return TryReadFlatVector(objectText, out vector,
                new[] { "x", "y", "z" },
                new[] { "X", "Y", "Z" });
        }

        private static bool TryReadFlatVector(string text, out Vector3 vector, params string[][] keyTriples)
        {
            vector = Vector3.zero;
            for (var i = 0; i < keyTriples.Length; i++)
            {
                var triple = keyTriples[i];
                if (triple == null || triple.Length < 3)
                {
                    continue;
                }

                if (TryReadNumber(text, out var x, triple[0])
                    && TryReadNumber(text, out var y, triple[1])
                    && TryReadNumber(text, out var z, triple[2]))
                {
                    vector = new Vector3((float)x, (float)y, (float)z);
                    return true;
                }
            }

            return false;
        }

        private static bool TryReadNumber(string text, out double value, params string[] keys)
        {
            value = 0.0;
            for (var i = 0; i < keys.Length; i++)
            {
                var pattern = QuoteKey(keys[i]) + @"\s*:\s*(" + NumberPattern + @")";
                var match = Regex.Match(text, pattern, Options);
                if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryBuildVector(string xText, string yText, string zText, out Vector3 vector)
        {
            vector = Vector3.zero;
            if (!float.TryParse(xText, NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                || !float.TryParse(yText, NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                || !float.TryParse(zText, NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                return false;
            }

            vector = new Vector3(x, y, z);
            return true;
        }

        private static bool TryExtractObject(string text, string key, out string objectText)
        {
            objectText = string.Empty;
            var keyPattern = QuoteKey(key) + @"\s*:\s*\{";
            var match = Regex.Match(text, keyPattern, Options);
            if (!match.Success)
            {
                return false;
            }

            var openIndex = text.IndexOf('{', match.Index + match.Length - 1);
            if (openIndex < 0)
            {
                return false;
            }

            var depth = 0;
            var inString = false;
            var escaped = false;
            for (var i = openIndex; i < text.Length; i++)
            {
                var c = text[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        objectText = text.Substring(openIndex, i - openIndex + 1);
                        return true;
                    }
                }
            }

            return false;
        }

        private static string QuoteKey(string key)
        {
            return "\\\"" + Regex.Escape(key) + "\\\"";
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
        }
    }
}
