using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WaterLevelSensorEmulator
{
    class Program
    {
        private static string _devicePrefix = "";
        private static int _numberOfDevices = 5;
        private static string _iotHubConnectionString = "";
        private static string _iotHubHostName = "";

        private static int _maxLocationId = 29880 + 1; //Random doesn't include upper bound
        private static int _maxWaterLevelSensorReading = 512 + 1; // Random doesn't include upper bound
        private static RegistryManager _registryManager;
        private static string[] _cityData;
        private static List<WaterLevelSensorReading> _devices;
        private static List<ITargetBlock<string>> _runningBlocks;
        private static CancellationTokenSource _cancellationToken;
        private static Random _random;

        static void Main(string[] args)
        {
            _registryManager = RegistryManager.CreateFromConnectionString(_iotHubConnectionString);
            _cityData = File.ReadAllLines("CityData.csv");
            _devices = new List<WaterLevelSensorReading>();
            _runningBlocks = new List<ITargetBlock<string>>();
            _cancellationToken = new CancellationTokenSource();
            _random = new Random();
            for(var i=0; i< _numberOfDevices; i++)
            {
                //set information that needs set once
                var deviceId = $"{_devicePrefix}_{i + 1}";
                var device = new WaterLevelSensorReading(deviceId);
                var cityIdx = _random.Next(1, _maxLocationId);
                var cityDataLine = _cityData.First(c => c.StartsWith(cityIdx.ToString()));
                var tokens = cityDataLine.Split(',');
                device.Latitude = Convert.ToDouble(tokens[3]);
                device.Longitude = Convert.ToDouble(tokens[4]);
                device.AzureDevice = GetOrRegisterDeviceInAzureAsync(deviceId).GetAwaiter().GetResult();
                device.AzureDeviceClient = DeviceClient.Create(_iotHubHostName, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, device.AzureDevice.Authentication.SymmetricKey.PrimaryKey), Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                var rp = new Microsoft.Azure.Devices.Shared.TwinCollection();
                rp["Type"] = "WaterLevelSensor";
                rp["Latitude"] = device.Latitude;
                rp["Longitude"] = device.Longitude;
                rp["Firmware"] = "1.0.0";
                rp["SupportedMethods"] = "";
                var telemetryNested = new Microsoft.Azure.Devices.Shared.TwinCollection();
                telemetryNested[device.MessageSchema] = WaterLevelSensorReading.GetTelemetryObject();
                rp["Telemetry"] = telemetryNested;

                device.AzureDeviceClient.UpdateReportedPropertiesAsync(rp).GetAwaiter().GetResult();
                _devices.Add(device);

                //blocks take a random reading, then run indefinitely until cancelled
                var block = CreateEmulatedDeviceReading(_cancellationToken.Token);
                _runningBlocks.Add(block);
                block.Post(deviceId);
            }
            
            Console.ReadLine();
            _cancellationToken.Cancel();
            Console.WriteLine("Finished (press any key to continue)");
            Console.ReadLine();
        }

        private static ITargetBlock<string> CreateEmulatedDeviceReading(CancellationToken cancellationToken)
        {
            ActionBlock<string> retValue = null;
            retValue = new ActionBlock<string>(async (deviceIdentifier) => 
                {
                    var sensor = _devices.First(d => d.DeviceId == deviceIdentifier);
                    sensor.WaterLevel = _random.Next(_maxWaterLevelSensorReading);
                    Console.WriteLine("Writing: " + sensor.ToString());

                    var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(sensor.ToString()));
                    
                    await sensor.AzureDeviceClient.SendEventAsync(message);
                    
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
                    retValue.Post(deviceIdentifier);
                }, 
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken });

            return retValue;
        }

        private static async Task<Device> GetOrRegisterDeviceInAzureAsync(string deviceId)
        {
            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await _registryManager.GetDeviceAsync(deviceId);
            }
            return device;
        }
    }
}
