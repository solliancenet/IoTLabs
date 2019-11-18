using System;
using System.Collections.Generic;
using System.Linq;
using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventHubs;
using System.Text;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Newtonsoft.Json;
using WaterSensorPrediction.Shared.Models;
using WaterSensorPredictionFunction.Models;

namespace WaterSensorPredictionFunction
{
    public class Functions
    {
        private readonly IHttpClientFactory _httpClientFactoryLogicApp;
        private readonly IHttpClientFactory _httpClientFactoryScoringService;
        private readonly TelemetryClient _telemetryClient;

        // Use Dependency Injection to inject the HttpClientFactory services and App Insights configuration that were configured in Startup.cs.
        public Functions(IHttpClientFactory httpClientFactoryLogicApp, IHttpClientFactory httpClientFactoryScoringService,
            TelemetryConfiguration telemetryConfiguration)
        {
            _httpClientFactoryLogicApp = httpClientFactoryLogicApp;
            _httpClientFactoryScoringService = httpClientFactoryScoringService;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        [FunctionName("WaterLevelAnomalyFunction")]
        public async Task WaterLevelAnomalyFunction([IoTHubTrigger("messages/events", Connection = "IotHubConnectionString")]EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();
            var allTelemetry = new List<Telemetry>();

            foreach (var eventData in events)
            {
                // Deserialize message body into the Telemetry object.
                var telemetry = JsonConvert.DeserializeObject<Telemetry>(
                    Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count));

                allTelemetry.Add(telemetry);
            }

            try
            {
                // Group by Device ID and store the average sensor values.
                var devices = from telemetry in allTelemetry
                              group telemetry by new { telemetry.DeviceId, telemetry.ClusterId, telemetry.Month }
                    into deviceGroup
                              select new
                              {
                                  deviceGroup.Key.DeviceId,
                                  deviceGroup.Key.ClusterId,
                                  deviceGroup.Key.Month,
                                  AverageHumidity = deviceGroup.Average(x => x.Humidity),
                                  AverageTemperature = deviceGroup.Average(x => x.Temperature),
                                  AverageWaterLevel = deviceGroup.Average(x => x.WaterLevel),
                                  Count = deviceGroup.Count()
                              };

                var httpClientScoringService = _httpClientFactoryScoringService.CreateClient(NamedHttpClients.ScoringServiceClient);

                foreach (var device in devices)
                {
                    var prediction = new Prediction();
                    var payload = new double[5];
                    payload[0] = device.ClusterId;
                    payload[1] = device.Month;
                    payload[2] = device.AverageTemperature;
                    payload[3] = device.AverageHumidity;
                    payload[4] = device.AverageWaterLevel;

                    var postBody = JsonConvert.SerializeObject(payload);
                    var scoringResponse = await httpClientScoringService.PostAsync(Environment.GetEnvironmentVariable("ScoringServiceUrl"), new StringContent(postBody, Encoding.UTF8, "application/json"));

                    if (scoringResponse.IsSuccessStatusCode)
                    {
                        prediction = await scoringResponse.Content.ReadAsAsync<Prediction>();
                    }

                    // Are there any anomalies in the scoring result?
                    if (prediction != null && prediction.Anomaly.Contains(true))
                    {
                        // Send an alert.
                        log.LogWarning($"Anomaly detected within the following device: '{device.DeviceId}'");

                        // Track anomaly and device details in Application Insights.
                        var properties = new Dictionary<string, string>
                        {
                            { "DeviceId", device.DeviceId },
                            { "ClusterId", device.ClusterId.ToString() },
                            { "Month", device.Month.ToString() },
                            { "loss_mae", prediction.LossMae.ToString() }
                        };
                        _telemetryClient.TrackEvent("Anomaly", properties);

                        // When sending metrics with GetMetric, the value is not sent right away.
                        // It is aggregated with other values for the same metric, and the resulting summary (aka "aggregate" is sent automatically every minute.
                        _telemetryClient.GetMetric($"Cluster{device.ClusterId}Anomalies").TrackValue(1);
                    }

                }
            }
            catch (Exception e)
            {
                // We need to keep processing the rest of the batch - capture this exception and continue.
                // Also, consider capturing details of the message that failed processing so it can be processed again later.
                exceptions.Add(e);
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that 
            // there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}