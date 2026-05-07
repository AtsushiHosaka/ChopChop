# BLE To UDP Bridge

This script connects to the ESP32 BLE peripheral and forwards every notify payload to Unity over UDP.

Unity side:
- `HashiInputProvider.Input Mode = Udp`
- `udpPort = 5005`

Run:

```bash
swift Tools/BleToUdpBridge/ble_to_udp.swift
```

Optional arguments:

```bash
swift Tools/BleToUdpBridge/ble_to_udp.swift \
  --device-name ESP32_IMU_BLE \
  --service-uuid 12345678-1234-1234-1234-1234567890ab \
  --characteristic-uuid abcd1234-1234-1234-1234-abcdef123456 \
  --udp-host 127.0.0.1 \
  --udp-port 5005
```

Payload format:
- Raw BLE notify payload is forwarded as-is.
- Unity currently accepts:
  - JSON: `{"theta":12.3,"pinch":80,"ax":0.1,"ay":-0.9,"az":0.2,"gx":1.2,"gy":0.0,"gz":-0.4}`
  - CSV: `ax,ay,az,gx,gy,gz`
  - CSV: `theta,pinch,ax,ay,az,gx,gy,gz`

Important:
- If you only send `ax,ay,az,gx,gy,gz`, Unity gets orientation data but not a real chopstick open/close signal.
- For actual chopstick opening gameplay, send either JSON or 8-field CSV with `theta` and `pinch`.
