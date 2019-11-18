using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace WaterSensorPredictionFunction.Models
{
    public class Prediction
    {
        [JsonProperty("loss_mae")]
        public double[] LossMae { get; set; }
        [JsonProperty("anomaly_std")]
        public bool[] Anomaly { get; set; }
    }
}
