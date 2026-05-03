using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ChopChop.BLE
{
    public sealed class MacHashiBleReceiver : IDisposable
    {
        private readonly object queueLock = new object();
        private readonly Queue<string> rawPayloads = new Queue<string>();
        private readonly Queue<string> logs = new Queue<string>();

        private bool isDisposed;

        public bool IsRunning { get; private set; }
        public string Status { get; private set; } = "Stopped";

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private static readonly NativeStringCallback DataCallback = HandleNativeData;
        private static readonly NativeStringCallback LogCallback = HandleNativeLog;
        private static readonly NativeStateCallback StateCallback = HandleNativeState;
        private static MacHashiBleReceiver activeReceiver;

        public void Start(string deviceNameFilter, string serviceUuid, string characteristicUuid)
        {
            ThrowIfDisposed();

            lock (queueLock)
            {
                rawPayloads.Clear();
                logs.Clear();
            }

            activeReceiver = this;

            try
            {
                Native.ChopChopBle_SetCallbacks(DataCallback, LogCallback, StateCallback);
                Native.ChopChopBle_Start(
                    Clean(deviceNameFilter),
                    Clean(serviceUuid),
                    Clean(characteristicUuid));

                IsRunning = true;
                Status = "Starting";
            }
            catch (DllNotFoundException exception)
            {
                IsRunning = false;
                Status = "Native plugin missing";
                ClearActiveReceiver();
                EnqueueLog($"Native BLE plugin was not found: {exception.Message}");
            }
            catch (EntryPointNotFoundException exception)
            {
                IsRunning = false;
                Status = "Native plugin entry point missing";
                ClearActiveReceiver();
                EnqueueLog($"Native BLE plugin entry point was not found: {exception.Message}");
            }
            catch (Exception exception)
            {
                IsRunning = false;
                Status = "Native plugin start failed";
                ClearActiveReceiver();
                EnqueueLog($"Native BLE plugin failed to start: {exception.Message}");
            }
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                ClearActiveReceiver();
                return;
            }

            try
            {
                Native.ChopChopBle_Stop();
            }
            catch (Exception exception)
            {
                EnqueueLog($"Native BLE plugin failed to stop cleanly: {exception.Message}");
            }
            finally
            {
                IsRunning = false;
                Status = "Stopped";
                ClearActiveReceiver();
            }
        }
#else
        public void Start(string deviceNameFilter, string serviceUuid, string characteristicUuid)
        {
            ThrowIfDisposed();
            IsRunning = false;
            Status = "Unsupported platform";
            EnqueueLog("macOS BLE is only available in the macOS Editor or macOS Standalone player.");
        }

        public void Stop()
        {
            IsRunning = false;
            Status = "Stopped";
        }
#endif

        public bool TryDequeueRawPayload(out string json)
        {
            lock (queueLock)
            {
                if (rawPayloads.Count == 0)
                {
                    json = string.Empty;
                    return false;
                }

                json = rawPayloads.Dequeue();
                return true;
            }
        }

        public bool TryDequeueLog(out string message)
        {
            lock (queueLock)
            {
                if (logs.Count == 0)
                {
                    message = string.Empty;
                    return false;
                }

                message = logs.Dequeue();
                return true;
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            Stop();
            isDisposed = true;
        }

        private void EnqueuePayload(string json)
        {
            lock (queueLock)
            {
                rawPayloads.Enqueue(json);
            }
        }

        private void EnqueueLog(string message)
        {
            lock (queueLock)
            {
                logs.Enqueue(message);
            }
        }

        private void SetNativeState(int state, string message)
        {
            Status = string.IsNullOrEmpty(message)
                ? ((NativeBleState)state).ToString()
                : $"{(NativeBleState)state}: {message}";
            EnqueueLog(Status);
        }

        private void ThrowIfDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(MacHashiBleReceiver));
            }
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private static void HandleNativeData(IntPtr payload)
        {
            var receiver = activeReceiver;
            if (receiver == null)
            {
                return;
            }

            receiver.EnqueuePayload(PtrToString(payload));
        }

        private static void HandleNativeLog(IntPtr message)
        {
            var receiver = activeReceiver;
            if (receiver == null)
            {
                Debug.Log($"[Hashi BLE] {PtrToString(message)}");
                return;
            }

            receiver.EnqueueLog(PtrToString(message));
        }

        private static void HandleNativeState(int state, IntPtr message)
        {
            var receiver = activeReceiver;
            if (receiver == null)
            {
                return;
            }

            receiver.SetNativeState(state, PtrToString(message));
        }

        private static string PtrToString(IntPtr pointer)
        {
            return pointer == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(pointer) ?? string.Empty;
        }

        private void ClearActiveReceiver()
        {
            if (activeReceiver == this)
            {
                activeReceiver = null;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeStringCallback(IntPtr message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void NativeStateCallback(int state, IntPtr message);

        private static class Native
        {
            private const string LibraryName = "ChopChopBleMac";

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void ChopChopBle_SetCallbacks(
                NativeStringCallback dataCallback,
                NativeStringCallback logCallback,
                NativeStateCallback stateCallback);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void ChopChopBle_Start(
                [MarshalAs(UnmanagedType.LPStr)] string deviceNameFilter,
                [MarshalAs(UnmanagedType.LPStr)] string serviceUuid,
                [MarshalAs(UnmanagedType.LPStr)] string characteristicUuid);

            [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
            internal static extern void ChopChopBle_Stop();
        }
#endif

        private enum NativeBleState
        {
            Stopped = 0,
            Scanning = 1,
            Connecting = 2,
            DiscoveringServices = 3,
            DiscoveringCharacteristics = 4,
            Subscribed = 5,
            Unsupported = 6,
            Unauthorized = 7,
            PoweredOff = 8,
            Failed = 9
        }
    }
}
