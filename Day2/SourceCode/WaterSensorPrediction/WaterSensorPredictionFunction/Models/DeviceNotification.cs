using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace WaterSensorPredictionFunction.Models
{
    public class DeviceNotification : TableEntity
    {
        public DeviceNotification()
        {
            this.PartitionKey = "Devices";
            this.RowKey = "Unknown";
        }

        public DeviceNotification(string deviceId)
        {
            this.PartitionKey = "Devices";
            this.RowKey = deviceId;
        }
        public DateTime LastNotificationUtc { get; set; }
    }
}
