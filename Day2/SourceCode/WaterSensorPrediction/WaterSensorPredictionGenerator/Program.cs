using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WaterSensorPrediction.Shared.Models;

namespace WaterSensorPredictionGenerator
{
    internal class Program
    {
        private static IConfigurationRoot _configuration;
        private static readonly object LockObject = new object();
        private static List<SimulatedSensor> _simulatedSensors = new List<SimulatedSensor>();
        // AutoResetEvent to signal when to exit the application.
        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);
        private static Dictionary<string, Task> _runningSensorTasks;

        // Extract device transaction data from the sample CSV file, serialize, and return the collection.
        private static List<Telemetry> GetTelemetryData(Func<string, string, Telemetry> factory)
        {
            var telemetry = new List<Telemetry>();

            Console.WriteLine("Retrieving sample transaction data...");

            using (var reader = new StreamReader(File.OpenRead(@"sensor_data.csv")))
            {
                var header = reader.ReadLines()
                    .First();
                var lines = reader.ReadLines()
                    .Skip(1);

                // Instantiate a Transaction object from the CSV line and header data, using the passed in factory:
                telemetry.AddRange(lines.Select(line => factory(line, header)));
            }

            Console.WriteLine($"Sample telemetry data retrieved. {telemetry.Count} records found.");

            return telemetry;
        }

        /// <summary>
        /// Extracts properties from either the appsettings.json file or system environment variables.
        /// </summary>
        /// <returns>
        /// IoTHubConnectionString: Connection string to IoT Hub for sending data.
        /// IoTHubHostName: The name of your IoT Hub service.
        /// </returns>
        private static (string IoTHubConnectionString, string IoTHubHostName) ParseArguments()
        {
            try
            {
                // The Configuration object will extract values either from the machine's environment variables, or the appsettings.json file.
                var ioTHubConnectionString = _configuration["IOT_HUB_CONNECTION_STRING"];
                var ioTHubHostName = _configuration["IOT_HUB_HOST_NAME"];

                if (string.IsNullOrWhiteSpace(ioTHubConnectionString))
                {
                    throw new ArgumentException("IOT_HUB_CONNECTION_STRING must be provided");
                }

                return (ioTHubConnectionString, ioTHubHostName);
            }
            catch (Exception e)
            {
                WriteLineInColor(e.Message, ConsoleColor.Red);
                Console.ReadLine();
                throw;
            }
        }

        static async Task Main(string[] args)
        {
            // Setup configuration to either read from the appsettings.json file (if present) or environment variables.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            var arguments = ParseArguments();

            var cancellationSource = new CancellationTokenSource();
            var cancellationToken = cancellationSource.Token;

            WriteLineInColor("Water Sensor Generator", ConsoleColor.White);
            Console.WriteLine("======");
            WriteLineInColor("Press Ctrl+C or Ctrl+Break to cancel.", ConsoleColor.Cyan);
            Console.WriteLine(string.Empty);

            // Handle Control+C or Control+Break.
            Console.CancelKeyPress += (o, e) =>
            {
                WriteLineInColor("Stopped generator. No more events are being sent.", ConsoleColor.Yellow);
                cancellationSource.Cancel();

                // Allow the main thread to continue and exit...
                WaitHandle.Set();
            };

            try
            {
                // Get telemetry data.
                var telemetry = GetTelemetryData(Telemetry.FromString);
                // Reverse the list so anomalies occur sooner.
                telemetry.Reverse();
                // Start sending telemetry from simulated sensor devices to IoT Hub:
                _runningSensorTasks = await SetupSensoreTelemetryRunTasks(telemetry, arguments.IoTHubConnectionString);
                var tasks = _runningSensorTasks.Select(t => t.Value).ToList();
                while (tasks.Count > 0)
                {
                    try
                    {
                        Task.WhenAll(tasks).Wait(cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        //expected
                    }

                    tasks = _runningSensorTasks.Where(t => !t.Value.IsCompleted).Select(t => t.Value).ToList();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("The sensor telemetry operation was canceled.");
                // No need to throw, as this was expected.
            }

            // Closing out...
            cancellationSource.Cancel();
            Console.WriteLine();
            WriteLineInColor("Done sending generated transaction data", ConsoleColor.Cyan);
            Console.WriteLine();
            Console.WriteLine();

            // Keep the console open.
            Console.ReadLine();
            WaitHandle.WaitOne();
        }

        /// <summary>
        /// Creates the set of tasks that will send telemetry data to IoT Hub.
        /// </summary>
        /// <returns></returns>
        private static async Task<Dictionary<string, Task>> SetupSensoreTelemetryRunTasks(IReadOnlyCollection<Telemetry> telemetry, string iotHubConnectionString)
        {
            var sensorTelemetryRunTasks = new Dictionary<string, Task>();
            
            var devices = from device in telemetry group device by device.DeviceId
                into deviceGroup select new
            {
                    DeviceId = deviceGroup.Key,
                    SensorData = deviceGroup.ToList()
            };

            var deviceList = devices.ToList();
            WriteLineInColor($"\nFound {telemetry.Count} sensor readings from {deviceList.Count} devices. Creating and registering devices...", ConsoleColor.Cyan);

            var deviceNumber = 1;

            foreach (var device in deviceList)
            {
                // Register vehicle IoT device, using its VIN as the device ID, then return the device key.
                var deviceKey = await DeviceManager.RegisterDevicesAsync(iotHubConnectionString, device.DeviceId);

                // Add the simulated sensor, acting as an AMQP device, and configure it with its data.
                _simulatedSensors.Add(new SimulatedSensor(device.SensorData, deviceNumber,
                    DeviceManager.HostName, device.DeviceId, deviceKey));
            }


            foreach (var simulatedSensor in _simulatedSensors)
            {
                sensorTelemetryRunTasks.Add(simulatedSensor.DeviceId, simulatedSensor.RunSensorSimulationAsync());
            }

            return sensorTelemetryRunTasks;
        }

        public static void WriteLineInColor(string msg, ConsoleColor color)
        {
            lock (LockObject)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }
    }
}
