using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using WaterSensorPrediction.Shared.Models;

namespace WaterSensorPredictionGenerator
{
    public class SimulatedSensor
    {
        // The amount of time to delay between sending telemetry.
        private readonly TimeSpan CycleTime = TimeSpan.FromMilliseconds(100);
        private DeviceClient _DeviceClient;
        private string _IotHubUri { get; set; }
        public string DeviceId { get; set; }
        public string DeviceKey { get; set; }
        private const string TelemetryEventHubName = "telemetry";
        private int _messagesSent = 0;
        private readonly int _deviceNumber = 0;
        private readonly CancellationTokenSource _localCancellationSource = new CancellationTokenSource();

        private readonly List<Telemetry> _sensorData;

        public int MessagesSent => _messagesSent;

        public SimulatedSensor(List<Telemetry> sensorData, int deviceNumber,
            string iotHubUri, string deviceId, string deviceKey)
        {
            _sensorData = sensorData;
            _deviceNumber = deviceNumber;
            _IotHubUri = iotHubUri;
            DeviceId = deviceId;
            DeviceKey = deviceKey;
            _DeviceClient = DeviceClient.Create(_IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey));
        }

        /// <summary>
        /// Creates an asynchronous task for sending all data for the sensor.
        /// </summary>
        /// <returns>Task for asynchronous device operation</returns>
        public async Task RunSensorSimulationAsync()
        {
            await SendDataToHub(_localCancellationSource.Token).ConfigureAwait(false);
        }

        public void CancelCurrentRun()
        {
            _localCancellationSource.Cancel();
        }

        /// <summary>
        /// Takes a set of sensor data for a device in a dataset and sends the
        /// data to the message with a configurable delay between each message.
        /// </summary>
        /// <returns></returns>
        private async Task SendDataToHub(CancellationToken cancellationToken)
        {
            var telemetryTimer = new Stopwatch();

            telemetryTimer.Start();

            while (!_localCancellationSource.IsCancellationRequested)
            {
                foreach (var sensorReading in _sensorData)
                {
                    // Serialize data and send to Event Hubs:
                    await SendEvent(JsonConvert.SerializeObject(sensorReading), cancellationToken).ConfigureAwait(false);

                    await Task.Delay(CycleTime, cancellationToken).ConfigureAwait(false);
                }

                
            }

            telemetryTimer.Stop();
        }

        /// <summary>
        /// Uses the EventHubClient to send a message to Event Hubs.
        /// </summary>
        /// <param name="message">JSON string representing serialized telemetry data.</param>
        /// <returns>Task for async execution.</returns>
        private async Task SendEvent(string message, CancellationToken cancellationToken)
        {
            using (var eventData = new Message(Encoding.ASCII.GetBytes(message)))
            {

                // Send telemetry to IoT Hub. All messages are partitioned by the Device Id, guaranteeing message ordering.
                var sendEventAsync = _DeviceClient?.SendEventAsync(eventData, cancellationToken);
                if (sendEventAsync != null) await sendEventAsync.ConfigureAwait(false);

                // Keep track of messages sent and update progress periodically.
                var currCount = Interlocked.Increment(ref _messagesSent);
                if (currCount % 50 == 0)
                {
                    Console.WriteLine($"Device {_deviceNumber}: Message count: {currCount}");
                }
            }
        }
    }
}
