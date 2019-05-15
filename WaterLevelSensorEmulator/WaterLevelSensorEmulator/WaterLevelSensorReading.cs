using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace WaterLevelSensorEmulator
{
    public class WaterLevelSensorReading
    {
        public string MessageSchema { get => "waterlevelsensors;v1"; }
        public string DeviceId { get; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int WaterLevel { get; set; }

        public Device AzureDevice { get; set; }
        public DeviceClient AzureDeviceClient { get; set; }
       

        public WaterLevelSensorReading(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        public override string ToString()
        {
            var serObj = new {
                waterlevel = this.WaterLevel
            };
            return JsonConvert.SerializeObject(serObj);
        }

        public static object GetTelemetryObject()
        {
            return new { 
                    Interval = "00:00:05",
                    MessageTemplate = "{waterlevel: ${waterlevel}}",
                    MessageSchema = new
                    {
                        Name = "waterlevelsensors;v1",
                        Format = "JSON",
                        Fields = new {
                            waterlevel = "Integer"
                        }
                    }
            };
        }
    }

    
}
