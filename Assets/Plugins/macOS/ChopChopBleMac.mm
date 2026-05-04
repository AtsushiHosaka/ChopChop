#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>
#include <string.h>

typedef void (*ChopChopBleStringCallback)(const char *message);
typedef void (*ChopChopBleStateCallback)(int state, const char *message);

typedef NS_ENUM(NSInteger, ChopChopBleState) {
    ChopChopBleStateStopped = 0,
    ChopChopBleStateScanning = 1,
    ChopChopBleStateConnecting = 2,
    ChopChopBleStateDiscoveringServices = 3,
    ChopChopBleStateDiscoveringCharacteristics = 4,
    ChopChopBleStateSubscribed = 5,
    ChopChopBleStateUnsupported = 6,
    ChopChopBleStateUnauthorized = 7,
    ChopChopBleStatePoweredOff = 8,
    ChopChopBleStateFailed = 9
};

static ChopChopBleStringCallback gDataCallback = nullptr;
static ChopChopBleStringCallback gLogCallback = nullptr;
static ChopChopBleStateCallback gStateCallback = nullptr;

static NSString *StringFromCString(const char *value)
{
    if (value == nullptr || strlen(value) == 0) {
        return @"";
    }

    NSString *string = [NSString stringWithUTF8String:value];
    return string ?: @"";
}

static void SendString(ChopChopBleStringCallback callback, NSString *message)
{
    if (callback == nullptr || message == nil) {
        return;
    }

    callback([message UTF8String]);
}

static void SendLog(NSString *message)
{
    SendString(gLogCallback, message);
}

static void SendState(ChopChopBleState state, NSString *message)
{
    if (gStateCallback == nullptr) {
        return;
    }

    gStateCallback((int)state, [message ?: @"" UTF8String]);
}

static CBUUID *UuidFromString(NSString *value)
{
    if (value.length == 0) {
        return nil;
    }

    @try {
        return [CBUUID UUIDWithString:value];
    } @catch (NSException *exception) {
        SendLog([NSString stringWithFormat:@"Invalid UUID '%@': %@", value, exception.reason]);
        return nil;
    }
}

static NSString *CharacteristicPropertiesString(CBCharacteristicProperties properties)
{
    NSMutableArray<NSString *> *parts = [NSMutableArray array];

    if ((properties & CBCharacteristicPropertyBroadcast) != 0) {
        [parts addObject:@"broadcast"];
    }
    if ((properties & CBCharacteristicPropertyRead) != 0) {
        [parts addObject:@"read"];
    }
    if ((properties & CBCharacteristicPropertyWriteWithoutResponse) != 0) {
        [parts addObject:@"writeWithoutResponse"];
    }
    if ((properties & CBCharacteristicPropertyWrite) != 0) {
        [parts addObject:@"write"];
    }
    if ((properties & CBCharacteristicPropertyNotify) != 0) {
        [parts addObject:@"notify"];
    }
    if ((properties & CBCharacteristicPropertyIndicate) != 0) {
        [parts addObject:@"indicate"];
    }
    if ((properties & CBCharacteristicPropertyAuthenticatedSignedWrites) != 0) {
        [parts addObject:@"signedWrite"];
    }
    if ((properties & CBCharacteristicPropertyExtendedProperties) != 0) {
        [parts addObject:@"extended"];
    }
    if ((properties & CBCharacteristicPropertyNotifyEncryptionRequired) != 0) {
        [parts addObject:@"notifyEncryptionRequired"];
    }
    if ((properties & CBCharacteristicPropertyIndicateEncryptionRequired) != 0) {
        [parts addObject:@"indicateEncryptionRequired"];
    }

    return parts.count == 0 ? @"none" : [parts componentsJoinedByString:@","];
}

@interface ChopChopBleCentral : NSObject <CBCentralManagerDelegate, CBPeripheralDelegate>
@property(nonatomic, strong) CBCentralManager *centralManager;
@property(nonatomic, strong) CBPeripheral *peripheral;
@property(nonatomic, strong) CBUUID *targetServiceUuid;
@property(nonatomic, strong) CBUUID *targetCharacteristicUuid;
@property(nonatomic, copy) NSString *deviceNameFilter;
@property(nonatomic, strong) NSMutableSet<NSString *> *loggedDeviceIds;
@property(nonatomic, strong) NSMutableSet<NSString *> *loggedCharacteristicIds;
@property(nonatomic, assign) BOOL scanRequested;
@end

@implementation ChopChopBleCentral

- (instancetype)init
{
    self = [super init];
    if (self) {
        _loggedDeviceIds = [NSMutableSet set];
        _loggedCharacteristicIds = [NSMutableSet set];
    }
    return self;
}

- (void)startWithDeviceName:(NSString *)deviceNameFilter
                serviceUuid:(NSString *)serviceUuid
         characteristicUuid:(NSString *)characteristicUuid
{
    [self stop];

    self.deviceNameFilter = deviceNameFilter ?: @"";
    self.targetServiceUuid = UuidFromString(serviceUuid ?: @"");
    self.targetCharacteristicUuid = UuidFromString(characteristicUuid ?: @"");
    self.scanRequested = YES;
    [self.loggedDeviceIds removeAllObjects];
    [self.loggedCharacteristicIds removeAllObjects];

    NSDictionary *options = @{ CBCentralManagerOptionShowPowerAlertKey: @YES };
    self.centralManager = [[CBCentralManager alloc] initWithDelegate:self
                                                               queue:dispatch_get_main_queue()
                                                             options:options];

    SendLog([NSString stringWithFormat:@"Starting macOS BLE scan. deviceName='%@' serviceUuid='%@' characteristicUuid='%@'",
             self.deviceNameFilter,
             serviceUuid ?: @"",
             characteristicUuid ?: @""]);
}

- (void)stop
{
    self.scanRequested = NO;

    if (self.centralManager != nil) {
        [self.centralManager stopScan];

        if (self.peripheral != nil) {
            [self.centralManager cancelPeripheralConnection:self.peripheral];
        }
    }

    self.peripheral.delegate = nil;
    self.peripheral = nil;
    self.centralManager.delegate = nil;
    self.centralManager = nil;

    SendState(ChopChopBleStateStopped, @"Stopped");
}

- (void)centralManagerDidUpdateState:(CBCentralManager *)central
{
    switch (central.state) {
        case CBManagerStatePoweredOn:
            [self beginScanIfReady];
            break;
        case CBManagerStatePoweredOff:
            SendState(ChopChopBleStatePoweredOff, @"Bluetooth is powered off.");
            break;
        case CBManagerStateUnauthorized:
            SendState(ChopChopBleStateUnauthorized, @"Bluetooth permission is not authorized for this app.");
            break;
        case CBManagerStateUnsupported:
            SendState(ChopChopBleStateUnsupported, @"Bluetooth LE is not supported on this Mac.");
            break;
        case CBManagerStateResetting:
            SendState(ChopChopBleStateFailed, @"Bluetooth manager is resetting.");
            break;
        case CBManagerStateUnknown:
        default:
            SendLog(@"Bluetooth manager state is unknown.");
            break;
    }
}

- (void)beginScanIfReady
{
    if (!self.scanRequested || self.centralManager.state != CBManagerStatePoweredOn) {
        return;
    }

    NSArray<CBUUID *> *services = self.targetServiceUuid != nil ? @[ self.targetServiceUuid ] : nil;
    NSDictionary *options = @{ CBCentralManagerScanOptionAllowDuplicatesKey: @NO };
    [self.centralManager scanForPeripheralsWithServices:services options:options];

    if (self.targetServiceUuid == nil && self.deviceNameFilter.length == 0) {
        SendState(ChopChopBleStateScanning, @"Scanning only. Set deviceNameFilter or serviceUuid to connect.");
    } else {
        SendState(ChopChopBleStateScanning, @"Scanning for matching peripherals.");
    }
}

- (void)centralManager:(CBCentralManager *)central
 didDiscoverPeripheral:(CBPeripheral *)peripheral
     advertisementData:(NSDictionary<NSString *, id> *)advertisementData
                  RSSI:(NSNumber *)RSSI
{
    NSString *advertisedName = advertisementData[CBAdvertisementDataLocalNameKey];
    NSString *name = peripheral.name.length > 0 ? peripheral.name : advertisedName;
    NSString *displayName = name.length > 0 ? name : @"(unnamed)";
    NSString *deviceId = peripheral.identifier.UUIDString;

    if (![self.loggedDeviceIds containsObject:deviceId]) {
        [self.loggedDeviceIds addObject:deviceId];
        SendLog([NSString stringWithFormat:@"Discovered BLE device: %@ id=%@ rssi=%@", displayName, deviceId, RSSI]);
    }

    BOOL hasNameFilter = self.deviceNameFilter.length > 0;
    BOOL hasServiceFilter = self.targetServiceUuid != nil;
    if (!hasNameFilter && !hasServiceFilter) {
        return;
    }

    if (hasNameFilter) {
        NSRange peripheralNameRange = [displayName rangeOfString:self.deviceNameFilter options:NSCaseInsensitiveSearch];
        NSRange advertisedNameRange = [advertisedName ?: @"" rangeOfString:self.deviceNameFilter options:NSCaseInsensitiveSearch];
        if (peripheralNameRange.location == NSNotFound && advertisedNameRange.location == NSNotFound) {
            return;
        }
    }

    self.peripheral = peripheral;
    self.peripheral.delegate = self;
    [self.centralManager stopScan];
    SendState(ChopChopBleStateConnecting, [NSString stringWithFormat:@"Connecting to %@.", displayName]);
    [self.centralManager connectPeripheral:peripheral options:nil];
}

- (void)centralManager:(CBCentralManager *)central didConnectPeripheral:(CBPeripheral *)peripheral
{
    SendState(ChopChopBleStateDiscoveringServices, [NSString stringWithFormat:@"Connected to %@. Discovering services.", peripheral.name ?: peripheral.identifier.UUIDString]);
    NSArray<CBUUID *> *services = self.targetServiceUuid != nil ? @[ self.targetServiceUuid ] : nil;
    [peripheral discoverServices:services];
}

- (void)centralManager:(CBCentralManager *)central didFailToConnectPeripheral:(CBPeripheral *)peripheral error:(NSError *)error
{
    SendState(ChopChopBleStateFailed, [NSString stringWithFormat:@"Failed to connect to %@: %@", peripheral.name ?: peripheral.identifier.UUIDString, error.localizedDescription]);
    self.peripheral = nil;
    [self beginScanIfReady];
}

- (void)centralManager:(CBCentralManager *)central didDisconnectPeripheral:(CBPeripheral *)peripheral error:(NSError *)error
{
    NSString *message = error != nil
        ? [NSString stringWithFormat:@"Disconnected from %@: %@", peripheral.name ?: peripheral.identifier.UUIDString, error.localizedDescription]
        : [NSString stringWithFormat:@"Disconnected from %@.", peripheral.name ?: peripheral.identifier.UUIDString];
    SendLog(message);
    self.peripheral = nil;
    [self beginScanIfReady];
}

- (void)peripheral:(CBPeripheral *)peripheral didDiscoverServices:(NSError *)error
{
    if (error != nil) {
        SendState(ChopChopBleStateFailed, [NSString stringWithFormat:@"Service discovery failed: %@", error.localizedDescription]);
        return;
    }

    if (peripheral.services.count == 0) {
        SendLog(@"No BLE services discovered.");
        return;
    }

    for (CBService *service in peripheral.services) {
        SendLog([NSString stringWithFormat:@"Discovered service: %@", service.UUID.UUIDString]);
        NSArray<CBUUID *> *characteristics = self.targetCharacteristicUuid != nil ? @[ self.targetCharacteristicUuid ] : nil;
        [peripheral discoverCharacteristics:characteristics forService:service];
    }
}

- (void)peripheral:(CBPeripheral *)peripheral
didDiscoverCharacteristicsForService:(CBService *)service
             error:(NSError *)error
{
    if (error != nil) {
        SendState(ChopChopBleStateFailed, [NSString stringWithFormat:@"Characteristic discovery failed for %@: %@", service.UUID.UUIDString, error.localizedDescription]);
        return;
    }

    for (CBCharacteristic *characteristic in service.characteristics) {
        NSString *key = [NSString stringWithFormat:@"%@/%@", service.UUID.UUIDString, characteristic.UUID.UUIDString];
        if (![self.loggedCharacteristicIds containsObject:key]) {
            [self.loggedCharacteristicIds addObject:key];
            SendLog([NSString stringWithFormat:@"Characteristic candidate: service=%@ characteristic=%@ properties=%@",
                     service.UUID.UUIDString,
                     characteristic.UUID.UUIDString,
                     CharacteristicPropertiesString(characteristic.properties)]);
        }

        if (self.targetCharacteristicUuid == nil) {
            continue;
        }

        if (![characteristic.UUID isEqual:self.targetCharacteristicUuid]) {
            continue;
        }

        BOOL canNotify = (characteristic.properties & CBCharacteristicPropertyNotify) != 0
            || (characteristic.properties & CBCharacteristicPropertyIndicate) != 0;
        BOOL canRead = (characteristic.properties & CBCharacteristicPropertyRead) != 0;

        if (canNotify) {
            SendState(ChopChopBleStateSubscribed, [NSString stringWithFormat:@"Subscribing to %@/%@.", service.UUID.UUIDString, characteristic.UUID.UUIDString]);
            [peripheral setNotifyValue:YES forCharacteristic:characteristic];
        } else if (canRead) {
            SendLog([NSString stringWithFormat:@"Characteristic %@/%@ is readable but not notifiable. Reading once.", service.UUID.UUIDString, characteristic.UUID.UUIDString]);
            [peripheral readValueForCharacteristic:characteristic];
        } else {
            SendState(ChopChopBleStateFailed, [NSString stringWithFormat:@"Characteristic %@/%@ cannot notify or read.", service.UUID.UUIDString, characteristic.UUID.UUIDString]);
        }
    }
}

- (void)peripheral:(CBPeripheral *)peripheral
didUpdateNotificationStateForCharacteristic:(CBCharacteristic *)characteristic
             error:(NSError *)error
{
    if (error != nil) {
        SendState(ChopChopBleStateFailed, [NSString stringWithFormat:@"Notification subscription failed for %@: %@", characteristic.UUID.UUIDString, error.localizedDescription]);
        return;
    }

    if (characteristic.isNotifying) {
        SendState(ChopChopBleStateSubscribed, [NSString stringWithFormat:@"Subscribed to characteristic %@.", characteristic.UUID.UUIDString]);
    } else {
        SendLog([NSString stringWithFormat:@"Notifications stopped for characteristic %@.", characteristic.UUID.UUIDString]);
    }
}

- (void)peripheral:(CBPeripheral *)peripheral
didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic
             error:(NSError *)error
{
    if (error != nil) {
        SendLog([NSString stringWithFormat:@"Characteristic update failed for %@: %@", characteristic.UUID.UUIDString, error.localizedDescription]);
        return;
    }

    NSData *data = characteristic.value;
    if (data.length == 0) {
        return;
    }

    NSString *payload = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    if (payload == nil) {
        payload = [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
    }

    if (payload == nil) {
        SendLog([NSString stringWithFormat:@"Characteristic %@ sent non-text data (%lu bytes).", characteristic.UUID.UUIDString, (unsigned long)data.length]);
        return;
    }

    SendString(gDataCallback, payload);
}

@end

static ChopChopBleCentral *gCentral = nil;

extern "C" void ChopChopBle_SetCallbacks(ChopChopBleStringCallback dataCallback,
                                         ChopChopBleStringCallback logCallback,
                                         ChopChopBleStateCallback stateCallback)
{
    gDataCallback = dataCallback;
    gLogCallback = logCallback;
    gStateCallback = stateCallback;
}

extern "C" void ChopChopBle_Start(const char *deviceNameFilter,
                                  const char *serviceUuid,
                                  const char *characteristicUuid)
{
    NSString *deviceName = StringFromCString(deviceNameFilter);
    NSString *service = StringFromCString(serviceUuid);
    NSString *characteristic = StringFromCString(characteristicUuid);

    dispatch_async(dispatch_get_main_queue(), ^{
        @autoreleasepool {
            if (gCentral == nil) {
                gCentral = [[ChopChopBleCentral alloc] init];
            }

            [gCentral startWithDeviceName:deviceName
                              serviceUuid:service
                       characteristicUuid:characteristic];
        }
    });
}

extern "C" void ChopChopBle_Stop()
{
    dispatch_async(dispatch_get_main_queue(), ^{
        @autoreleasepool {
            [gCentral stop];
            gCentral = nil;
        }
    });
}
