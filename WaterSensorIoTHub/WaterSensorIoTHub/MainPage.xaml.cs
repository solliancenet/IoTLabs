using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using System;
using System.Text;

namespace WaterSensorIoTHub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //AzureIoTHub.SendDeviceToCloudMessageAsync().Wait();
            SendMessageToIoTHubAsync(22);
           
        }


        private static async Task SendMessageToIoTHubAsync(int sensorReading)
        {
            /*
            TpmDevice myDevice = new TpmDevice(0); // Use logical device 0 on the TPM
            string hubUri = myDevice.GetHostName();
            string deviceId = myDevice.GetDeviceId();
            string sasToken = myDevice.GetSASToken();
            using (var deviceClient = DeviceClient.Create(
                hubUri,
                AuthenticationMethodFactory.
                    CreateAuthenticationWithToken(deviceId, sasToken), TransportType.Mqtt)){}
            */
            //TODO: see if implementing a background service will allow the use of the MQTT transport - determined UWP only seems to support Http1
            var connectionString = "HostName=IoTLabsHub.azure-devices.net;DeviceId=cpi3;SharedAccessKey=xvNO1KsgmEkWHFweYATBeGg069obtqbICop8wl09+nc=";


            using (var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1))
            {
                
                try
                {
                    var payload = "{" +
                        "\"deviceId\":\"cpi3\", " +
                        "\"sensorType\":\"Water Level\", " +
                        "\"sensorReading\":" + sensorReading + ", " +
                        "\"localTimestamp\":\"" + DateTime.Now.ToLocalTime() + "\"" +
                        "}";

                    var msg = new Message(Encoding.UTF8.GetBytes(payload));

                 

                    await deviceClient.SendEventAsync(msg);
                }
                catch (Exception ex)
                {
                    int i = 0;
                }
            }
            
           
        }

    }
}
