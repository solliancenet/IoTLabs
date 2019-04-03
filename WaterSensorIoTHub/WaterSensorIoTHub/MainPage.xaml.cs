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
        private static DeviceClient _deviceClient = null;
        public MainPage()
        {
            this.InitializeComponent();
            //AzureIoTHub.SendDeviceToCloudMessageAsync().Wait();
            var deviceId = "cpi3";
            var deviceKey = "xvNO1KsgmEkWHFweYATBeGg069obtqbICop8wl09+nc=";
            var hubUri = "IoTLabsHub.azure-devices.net";
            _deviceClient = DeviceClient.Create(hubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Http1);
            SendMessageToIoTHubAsync(84);
           
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
           
          

           
            try
            {
                var payload = "{" +
                    "\"deviceId\":\"cpi3\", " +
                    "\"sensorType\":\"Water Level\", " +
                    "\"sensorReading\":" + sensorReading + ", " +
                    "\"localTimestamp\":\"" + DateTime.Now.ToLocalTime() + "\"" +
                    "}";

                var msg = new Message(Encoding.UTF8.GetBytes(payload));

                 

                await _deviceClient.SendEventAsync(msg);
            }
            catch (Exception ex)
            {
                int i = 0;
            }
            
            
           
        }

    }
}
