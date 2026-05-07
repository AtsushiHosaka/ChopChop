import Foundation
import CoreBluetooth
import Network

final class BleToUdpBridge: NSObject, CBCentralManagerDelegate, CBPeripheralDelegate {
    private let deviceName: String
    private let serviceUuid: CBUUID
    private let characteristicUuid: CBUUID
    private let udpConnection: NWConnection
    private let queue = DispatchQueue(label: "chopchop.ble-to-udp")

    private var centralManager: CBCentralManager?
    private var peripheral: CBPeripheral?
    private var notifyCharacteristic: CBCharacteristic?

    init(deviceName: String, serviceUuid: String, characteristicUuid: String, udpHost: String, udpPort: UInt16) {
        self.deviceName = deviceName
        self.serviceUuid = CBUUID(string: serviceUuid)
        self.characteristicUuid = CBUUID(string: characteristicUuid)
        self.udpConnection = NWConnection(host: NWEndpoint.Host(udpHost), port: NWEndpoint.Port(rawValue: udpPort)!, using: .udp)
        super.init()
    }

    func start() {
        udpConnection.stateUpdateHandler = { state in
            switch state {
            case .ready:
                print("UDP ready")
            case .failed(let error):
                fputs("UDP failed: \(error)\n", stderr)
                exit(1)
            default:
                break
            }
        }
        udpConnection.start(queue: queue)

        centralManager = CBCentralManager(delegate: self, queue: queue)

        print("BLE -> UDP bridge started")
        print("deviceName=\(deviceName)")
        print("serviceUuid=\(serviceUuid.uuidString)")
        print("characteristicUuid=\(characteristicUuid.uuidString)")
    }

    func centralManagerDidUpdateState(_ central: CBCentralManager) {
        switch central.state {
        case .poweredOn:
            startScan()
        case .unauthorized:
            fputs("Bluetooth unauthorized. Allow terminal Bluetooth access in macOS settings.\n", stderr)
            exit(1)
        case .unsupported:
            fputs("Bluetooth unsupported on this Mac.\n", stderr)
            exit(1)
        case .poweredOff:
            fputs("Bluetooth is powered off.\n", stderr)
        default:
            break
        }
    }

    func centralManager(_ central: CBCentralManager,
                        didDiscover peripheral: CBPeripheral,
                        advertisementData: [String : Any],
                        rssi RSSI: NSNumber) {
        let candidateName = peripheral.name ?? (advertisementData[CBAdvertisementDataLocalNameKey] as? String) ?? ""
        guard candidateName == deviceName else {
            return
        }

        print("Discovered \(candidateName), connecting")
        self.peripheral = peripheral
        peripheral.delegate = self
        central.stopScan()
        central.connect(peripheral, options: nil)
    }

    func centralManager(_ central: CBCentralManager, didConnect peripheral: CBPeripheral) {
        print("Connected to \(peripheral.name ?? "unknown")")
        peripheral.discoverServices([serviceUuid])
    }

    func centralManager(_ central: CBCentralManager, didFailToConnect peripheral: CBPeripheral, error: Error?) {
        fputs("Failed to connect: \(error?.localizedDescription ?? "unknown")\n", stderr)
        restartScan(after: 1.0)
    }

    func centralManager(_ central: CBCentralManager, didDisconnectPeripheral peripheral: CBPeripheral, error: Error?) {
        notifyCharacteristic = nil
        if let error {
            fputs("Disconnected: \(error.localizedDescription)\n", stderr)
        } else {
            print("Disconnected")
        }
        restartScan(after: 0.5)
    }

    func peripheral(_ peripheral: CBPeripheral, didDiscoverServices error: Error?) {
        if let error {
            fputs("Service discovery failed: \(error.localizedDescription)\n", stderr)
            return
        }

        guard let services = peripheral.services else {
            return
        }

        for service in services where service.uuid == serviceUuid {
            peripheral.discoverCharacteristics([characteristicUuid], for: service)
        }
    }

    func peripheral(_ peripheral: CBPeripheral,
                    didDiscoverCharacteristicsFor service: CBService,
                    error: Error?) {
        if let error {
            fputs("Characteristic discovery failed: \(error.localizedDescription)\n", stderr)
            return
        }

        guard let characteristics = service.characteristics else {
            return
        }

        for characteristic in characteristics where characteristic.uuid == characteristicUuid {
            notifyCharacteristic = characteristic
            peripheral.setNotifyValue(true, for: characteristic)
            print("Subscribed to notify characteristic")
            return
        }

        fputs("Characteristic not found on service.\n", stderr)
    }

    func peripheral(_ peripheral: CBPeripheral,
                    didUpdateNotificationStateFor characteristic: CBCharacteristic,
                    error: Error?) {
        if let error {
            fputs("Notify state failed: \(error.localizedDescription)\n", stderr)
            return
        }

        print("Notify state: \(characteristic.isNotifying)")
    }

    func peripheral(_ peripheral: CBPeripheral,
                    didUpdateValueFor characteristic: CBCharacteristic,
                    error: Error?) {
        if let error {
            fputs("Value update failed: \(error.localizedDescription)\n", stderr)
            return
        }

        guard characteristic.uuid == characteristicUuid,
              let data = characteristic.value,
              !data.isEmpty,
              let payload = String(data: data, encoding: .utf8)?
                .trimmingCharacters(in: .whitespacesAndNewlines),
              !payload.isEmpty else {
            return
        }

        print(payload)
        udpConnection.send(content: Data(payload.utf8), completion: .contentProcessed { sendError in
            if let sendError {
                fputs("UDP send failed: \(sendError.localizedDescription)\n", stderr)
            }
        })
    }

    private func startScan() {
        print("Scanning for \(deviceName)")
        centralManager?.scanForPeripherals(withServices: [serviceUuid], options: [
            CBCentralManagerScanOptionAllowDuplicatesKey: false
        ])
    }

    private func restartScan(after delay: TimeInterval) {
        queue.asyncAfter(deadline: .now() + delay) { [weak self] in
            self?.startScan()
        }
    }
}

struct Options {
    var deviceName = "ESP32_IMU_BLE"
    var serviceUuid = "12345678-1234-1234-1234-1234567890ab"
    var characteristicUuid = "abcd1234-1234-1234-1234-abcdef123456"
    var udpHost = "127.0.0.1"
    var udpPort: UInt16 = 5005
}

func parseOptions(arguments: [String]) -> Options {
    var options = Options()
    var index = 1

    while index < arguments.count {
        switch arguments[index] {
        case "--device-name":
            index += 1
            if index < arguments.count { options.deviceName = arguments[index] }
        case "--service-uuid":
            index += 1
            if index < arguments.count { options.serviceUuid = arguments[index] }
        case "--characteristic-uuid":
            index += 1
            if index < arguments.count { options.characteristicUuid = arguments[index] }
        case "--udp-host":
            index += 1
            if index < arguments.count { options.udpHost = arguments[index] }
        case "--udp-port":
            index += 1
            if index < arguments.count, let port = UInt16(arguments[index]) { options.udpPort = port }
        case "--help":
            print("""
Usage:
  swift Tools/BleToUdpBridge/ble_to_udp.swift [options]

Options:
  --device-name         BLE peripheral name. Default: ESP32_IMU_BLE
  --service-uuid        BLE service UUID.
  --characteristic-uuid BLE characteristic UUID.
  --udp-host            UDP target host. Default: 127.0.0.1
  --udp-port            UDP target port. Default: 5005
""")
            exit(0)
        default:
            break
        }

        index += 1
    }

    return options
}

let options = parseOptions(arguments: CommandLine.arguments)
let bridge = BleToUdpBridge(
    deviceName: options.deviceName,
    serviceUuid: options.serviceUuid,
    characteristicUuid: options.characteristicUuid,
    udpHost: options.udpHost,
    udpPort: options.udpPort)

bridge.start()
RunLoop.main.run()
