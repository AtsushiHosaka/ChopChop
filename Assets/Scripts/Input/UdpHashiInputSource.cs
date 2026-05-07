using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using ChopChop.BLE;
using UnityEngine;

namespace ChopChop.Input
{
    public sealed class UdpHashiInputSource : IHashiInputSource
    {
        private static readonly Regex CsvSeparator = new Regex("\\s*,\\s*", RegexOptions.Compiled);
        private readonly object payloadLock = new object();
        private readonly string bindAddress;
        private readonly string ownerLabel;
        private readonly int port;
        private readonly float accelerometerCorrection;

        private UdpClient udpClient;
        private Thread receiveThread;
        private Quaternion rotation = Quaternion.identity;
        private string latestPayload = string.Empty;
        private bool hasPendingPayload;
        private bool running;
        private bool staleSampleLogEmitted;
        private float lastMalformedPayloadWarningAt = -999f;
        private int suppressedMalformedPayloadWarnings;

        public bool IsRunning => running;
        public HashiInputFrame CurrentFrame { get; private set; } = HashiInputFrame.Empty;

        public UdpHashiInputSource(int port, string bindAddress = "", string ownerLabel = "", float accelerometerCorrection = 0.015f)
        {
            this.port = Mathf.Max(1, port);
            this.bindAddress = bindAddress ?? string.Empty;
            this.ownerLabel = string.IsNullOrWhiteSpace(ownerLabel) ? "Hashi" : ownerLabel.Trim();
            this.accelerometerCorrection = Mathf.Clamp01(accelerometerCorrection);
        }

        public void StartInput()
        {
            StopInput();

            staleSampleLogEmitted = false;
            suppressedMalformedPayloadWarnings = 0;
            lastMalformedPayloadWarningAt = -999f;
            rotation = Quaternion.identity;
            latestPayload = string.Empty;
            hasPendingPayload = false;

            try
            {
                var endPoint = string.IsNullOrWhiteSpace(bindAddress)
                    ? new IPEndPoint(IPAddress.Any, port)
                    : new IPEndPoint(IPAddress.Parse(bindAddress), port);
                udpClient = new UdpClient(endPoint);
                running = true;
                receiveThread = new Thread(ReceiveLoop)
                {
                    IsBackground = true,
                    Name = $"HashiUdpReceiver-{ownerLabel}"
                };
                receiveThread.Start();
                Debug.Log($"[Hashi Input/{ownerLabel}] UDP receiver started on port {port}.");
            }
            catch (Exception exception)
            {
                running = false;
                udpClient?.Close();
                udpClient = null;
                Debug.LogError($"[Hashi Input/{ownerLabel}] Failed to start UDP receiver on port {port}: {exception.Message}");
            }
        }

        public void StopInput()
        {
            running = false;

            try
            {
                udpClient?.Close();
            }
            catch
            {
            }

            udpClient = null;

            if (receiveThread != null)
            {
                receiveThread.Join(200);
                receiveThread = null;
            }
        }

        public void Tick(float deltaTime)
        {
            string payload = null;
            lock (payloadLock)
            {
                if (hasPendingPayload)
                {
                    payload = latestPayload;
                    latestPayload = string.Empty;
                    hasPendingPayload = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(payload))
            {
                ConsumePayload(payload, deltaTime);
            }

            if (CurrentFrame.HasSample && !staleSampleLogEmitted && Time.realtimeSinceStartup - CurrentFrame.TimeReceived >= 2f)
            {
                Debug.LogWarning($"[Hashi Input/{ownerLabel}] No UDP sample received for {Time.realtimeSinceStartup - CurrentFrame.TimeReceived:0.0}s. Holding the last valid chopstick pose.");
                staleSampleLogEmitted = true;
            }
        }

        private void ReceiveLoop()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);

            while (running)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    var payload = System.Text.Encoding.UTF8.GetString(data);
                    lock (payloadLock)
                    {
                        latestPayload = payload;
                        hasPendingPayload = true;
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    if (!running)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    if (!running)
                    {
                        break;
                    }

                    Debug.LogWarning($"[Hashi Input/{ownerLabel}] UDP receive loop stopped: {exception.Message}");
                    break;
                }
            }

            running = false;
        }

        private void ConsumePayload(string payload, float deltaTime)
        {
            if (!TryParsePayload(payload, out var sample, out var error))
            {
                LogMalformedPayload(error);
                return;
            }

            var gyro = sample.HasGyroVector ? sample.GyroVector : Vector3.zero;
            if (deltaTime > 0f)
            {
                rotation *= Quaternion.Euler(gyro * deltaTime);
            }

            if (sample.HasAccelerationVector)
            {
                rotation = ApplyAccelerometerCorrection(rotation, sample.AccelerationVector);
            }

            var theta = sample.HasTheta ? sample.ThetaValue : CurrentFrame.ThetaDegrees;
            var pinch = sample.HasPinch
                ? Mathf.InverseLerp(0f, 200f, sample.PinchValue)
                : Mathf.InverseLerp(40f, 5f, theta);

            CurrentFrame = new HashiInputFrame(
                theta,
                pinch,
                sample.HasAccelerationVector ? sample.AccelerationVector : Vector3.zero,
                gyro,
                rotation,
                true,
                sample.RealtimeReceived);
            staleSampleLogEmitted = false;
        }

        private static bool TryParsePayload(string payload, out HashiBleSample sample, out string error)
        {
            if (HashiBleSampleParser.TryParse(payload, out sample, out error))
            {
                return true;
            }

            return TryParseCsvPayload(payload, out sample, out error);
        }

        private static bool TryParseCsvPayload(string payload, out HashiBleSample sample, out string error)
        {
            sample = HashiBleSample.Create(payload);
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(payload))
            {
                error = "Payload is empty.";
                return false;
            }

            var parts = CsvSeparator.Split(payload.Trim());
            if (parts.Length != 6 && parts.Length != 8)
            {
                error = "CSV payload must contain 6 values (ax,ay,az,gx,gy,gz) or 8 values (theta,pinch,ax,ay,az,gx,gy,gz).";
                return false;
            }

            var values = new float[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                {
                    error = $"CSV field {i} is not a valid number.";
                    return false;
                }
            }

            var index = 0;
            if (parts.Length == 8)
            {
                sample.SetTheta(values[index++]);
                sample.SetPinch(values[index++]);
            }

            sample.SetAx(values[index++]);
            sample.SetAy(values[index++]);
            sample.SetAz(values[index++]);
            sample.SetGx(values[index++]);
            sample.SetGy(values[index++]);
            sample.SetGz(values[index++]);
            return true;
        }

        private Quaternion ApplyAccelerometerCorrection(Quaternion currentRotation, Vector3 acceleration)
        {
            if (accelerometerCorrection <= 0f)
            {
                return currentRotation;
            }

            var magnitude = acceleration.magnitude;
            if (magnitude < 0.25f || magnitude > 3.5f)
            {
                return currentRotation;
            }

            var measuredUp = -acceleration.normalized;
            var currentUp = currentRotation * Vector3.up;
            if (currentUp.sqrMagnitude < 0.0001f || measuredUp.sqrMagnitude < 0.0001f)
            {
                return currentRotation;
            }

            var correctedRotation = Quaternion.FromToRotation(currentUp.normalized, measuredUp) * currentRotation;
            return Quaternion.Slerp(currentRotation, correctedRotation, accelerometerCorrection);
        }

        private void LogMalformedPayload(string error)
        {
            var now = Time.realtimeSinceStartup;
            if (now - lastMalformedPayloadWarningAt < 2f)
            {
                suppressedMalformedPayloadWarnings++;
                return;
            }

            var suffix = suppressedMalformedPayloadWarnings > 0
                ? $" Suppressed {suppressedMalformedPayloadWarnings} similar malformed payload warning(s)."
                : string.Empty;
            Debug.LogWarning($"[Hashi Input/{ownerLabel}] Ignored UDP payload: {error}.{suffix}");
            lastMalformedPayloadWarningAt = now;
            suppressedMalformedPayloadWarnings = 0;
        }
    }
}
