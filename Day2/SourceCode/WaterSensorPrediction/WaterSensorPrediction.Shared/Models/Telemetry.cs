using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WaterSensorPrediction.Shared.Models
{
    public class Telemetry
    {
        [JsonProperty]
        public string DeviceId { get; set; }
        [JsonProperty]
        public double Temperature { get; set; }
        [JsonProperty]
        public double Humidity { get; set; }
        [JsonProperty]
        public double WaterLevel { get; set; }
        [JsonProperty]
        public int ClusterId { get; set; }
        [JsonProperty]
        public int Month { get; set; }

        [JsonIgnore]
        protected string CsvHeader { get; set; }

        [JsonIgnore]
        protected string CsvString { get; set; }

        public string GetData()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Telemetry FromString(string line, string header)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new ArgumentException($"{nameof(line)} cannot be null, empty, or only whitespace");
            }

            var tokens = line.Split(',');
            if (tokens.Length != 6)
            {
                throw new ArgumentException($"Invalid record: {line}");
            }

            var tx = new Telemetry
            {
                CsvString = line,
                CsvHeader = header
            };
            try
            {
                tx.Month = int.TryParse(tokens[0], out var iresult) ? iresult : 0;
                tx.Temperature = double.TryParse(tokens[1], out var dresult) ? dresult : 0.0;
                tx.Humidity = double.TryParse(tokens[2], out dresult) ? dresult : 0.0;
                tx.WaterLevel = double.TryParse(tokens[3], out dresult) ? dresult : 0.0;
                tx.ClusterId = int.TryParse(tokens[4], out iresult) ? iresult : 0;
                tx.DeviceId = tokens[5];

                return tx;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid record: {line}", ex);
            }
        }
    }
}
