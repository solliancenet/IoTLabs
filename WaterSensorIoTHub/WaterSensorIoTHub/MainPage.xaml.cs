using Windows.UI.Xaml.Controls;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using System;
using System.Text;
using Windows.UI.Xaml;

namespace WaterSensorIoTHub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static DeviceClient _deviceClient = null;
        private string _deviceSasToken = "HostName=cep0516iothub.azure-devices.net;DeviceId=cpi3;SharedAccessSignature=SharedAccessSignature sr=cep0516iothub.azure-devices.net%2Fdevices%2Fcpi3&sig=iyGxDFsdFeVJ%2BQCPSezHRxqSnOzsiopl%2FWgkD4jKLpY%3D&se=1554597336";
        private string _deviceId = "cpi3";
        private MCP3008 _adc = new MCP3008();
        private DispatcherTimer _timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
            _deviceClient = DeviceClient.CreateFromConnectionString(_deviceSasToken, TransportType.Http1);
            Setup();
        }

        private async void Setup()
        {
            bool isConnected = await _adc.Connect();
            if (!isConnected)
            {
                txtStatus.Text = "There was a problem connecting to the MCP3008, please check your wiring";
            }
            else
            {
                btnStartSampling.IsEnabled = true;
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.Tick += timer_Tick;
            }
        }


        /// <summary>
        /// When actively sampling, obtain a voltage reading 
        /// </summary>
        /// <param name="sender">ignore</param>
        /// <param name="e">ignore</param>
        private async void timer_Tick(object sender, object e)
        {
            int vol = _adc.SampleVoltage(0);
            txtStatus.Text = "READING: " + vol.ToString();

            try
            {
                var payload = "{" +
                    "\"deviceId\":\"" + _deviceId + "\", " +
                    "\"sensorType\":\"Water Level\", " +
                    "\"sensorReading\":" + vol.ToString() + ", " +
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

        /// <summary>
        /// Begins the timer to sample voltage every 100ms
        /// </summary>
        /// <param name="sender">Start Button</param>
        /// <param name="e">ignore</param>
        private void btnStartSampling_Click(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        /// <summary>
        /// Stops the timer to suspend sampling voltage
        /// </summary>
        /// <param name="sender">Stop Button</param>
        /// <param name="e">ignore</param>
        private void btnEndSampling_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
        }

    }
}
