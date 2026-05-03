using System.Globalization;
using System.Text.RegularExpressions;

namespace ChopChop.BLE
{
    public static class HashiBleSampleParser
    {
        private static readonly Regex NumericFieldRegex = new Regex(
            "\"(?<key>[A-Za-z_][A-Za-z0-9_]*)\"\\s*:\\s*(?<value>[-+]?(?:\\d+\\.?\\d*|\\.\\d+)(?:[eE][-+]?\\d+)?)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static bool TryParse(string json, out HashiBleSample sample, out string error)
        {
            sample = HashiBleSample.Create(json);
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Payload is empty.";
                return false;
            }

            var trimmed = json.Trim();
            if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
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
    }
}
