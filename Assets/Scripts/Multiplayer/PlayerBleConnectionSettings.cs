using System;
using ChopChop.Core;
using UnityEngine;

namespace ChopChop.Multiplayer
{
    [Serializable]
    public struct PlayerBleConnectionSettings
    {
        [SerializeField] private PlayerId playerId;
        [SerializeField] private string playerLabel;
        [SerializeField] private string deviceNameFilter;
        [SerializeField] private string serviceUuid;
        [SerializeField] private string characteristicUuid;

        public PlayerId PlayerId => playerId;
        public string PlayerLabel => string.IsNullOrWhiteSpace(playerLabel) ? playerId.ToString() : playerLabel.Trim();
        public string DeviceNameFilter => Clean(deviceNameFilter);
        public string ServiceUuid => Clean(serviceUuid);
        public string CharacteristicUuid => Clean(characteristicUuid);
        public bool HasDeviceIdentity => !string.IsNullOrWhiteSpace(DeviceNameFilter) || !string.IsNullOrWhiteSpace(ServiceUuid);

        public PlayerBleConnectionSettings(PlayerId id, string label)
        {
            playerId = id;
            playerLabel = label;
            deviceNameFilter = string.Empty;
            serviceUuid = string.Empty;
            characteristicUuid = string.Empty;
        }

        public bool HasSameBleTarget(PlayerBleConnectionSettings other)
        {
            return string.Equals(DeviceNameFilter, other.DeviceNameFilter, StringComparison.OrdinalIgnoreCase)
                && string.Equals(ServiceUuid, other.ServiceUuid, StringComparison.OrdinalIgnoreCase)
                && string.Equals(CharacteristicUuid, other.CharacteristicUuid, StringComparison.OrdinalIgnoreCase);
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
