using ChopChop.BLE;
using ChopChop.Core;
using ChopChop.Input;
using UnityEngine;

namespace ChopChop.Multiplayer
{
    [AddComponentMenu("ChopChop/Multiplayer/Two Player BLE Assignment")]
    public sealed class TwoPlayerBleAssignment : MonoBehaviour
    {
        [SerializeField] private HashiInputProvider playerOneInput;
        [SerializeField] private HashiInputProvider playerTwoInput;
        [SerializeField] private PlayerBleConnectionSettings playerOne = new PlayerBleConnectionSettings(new PlayerId(0), "P1");
        [SerializeField] private PlayerBleConnectionSettings playerTwo = new PlayerBleConnectionSettings(new PlayerId(1), "P2");
        [SerializeField] private bool applyOnAwake;

        public PlayerBleConnectionSettings PlayerOne => playerOne;
        public PlayerBleConnectionSettings PlayerTwo => playerTwo;

        public bool ValidateProductionBle(out string error)
        {
            if (playerOneInput == null || playerTwoInput == null)
            {
                error = "Two-player BLE requires both P1 and P2 HashiInputProvider references.";
                return false;
            }

            if (!playerOne.HasDeviceIdentity || !playerTwo.HasDeviceIdentity)
            {
                error = "Two-player BLE requires each player to have a non-empty deviceNameFilter or serviceUuid. Empty scan mode is discovery-only and can mix players.";
                return false;
            }

            if (playerOne.HasSameBleTarget(playerTwo))
            {
                error = "P1 and P2 BLE settings point to the same target. Use unique deviceNameFilter values or a native receiver id before production play.";
                return false;
            }

            if (!MacHashiBleReceiver.SupportsConcurrentReceivers)
            {
                error = "The current macOS BLE plugin supports only one active receiver. Two-player production BLE needs the native multi-receiver upgrade.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        public void ApplyToProviders()
        {
            Apply(playerOneInput, playerOne);
            Apply(playerTwoInput, playerTwo);

            if (!ValidateProductionBle(out var error))
            {
                Debug.LogWarning($"[Two Player BLE] {error}");
            }
        }

        private void Awake()
        {
            if (applyOnAwake)
            {
                ApplyToProviders();
            }
        }

        private static void Apply(HashiInputProvider provider, PlayerBleConnectionSettings settings)
        {
            if (provider == null)
            {
                return;
            }

            provider.ConfigurePlayer(settings.PlayerId, settings.PlayerLabel);
            provider.ConfigureConnection(settings.DeviceNameFilter, settings.ServiceUuid, settings.CharacteristicUuid, true);
        }
    }
}
