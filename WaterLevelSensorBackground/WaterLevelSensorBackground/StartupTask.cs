using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics;

namespace WaterLevelSensorBackground
{
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private MCP3008 _adc;
        private ThreadPoolTimer _timer;
        private DeviceClient _deviceClient = null;
        private string _deviceSasToken = "";
        private string _deviceId = "";

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;

            _adc = new MCP3008();
            
            if (_adc.IsConnected)
            {
                _deviceClient = DeviceClient.CreateFromConnectionString(_deviceSasToken, TransportType.Http1);
                _timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromSeconds(5));
                
            }
        }

        private async void Timer_Tick(ThreadPoolTimer timer)
        {
            try
            {
                var voltage = _adc.SampleVoltage(0);
                Debug.WriteLine("Reading: " + voltage.ToString());
                var payload = "{" +
                   "\"deviceId\":\"" + _deviceId + "\", " +
                   "\"sensorType\":\"Water Level\", " +
                   "\"sensorReading\":" + voltage.ToString() + ", " +
                   "\"localTimestamp\":\"" + DateTime.Now.ToLocalTime() + "\"" +
                   "}";

                var msg = new Message(Encoding.UTF8.GetBytes(payload));

                await _deviceClient.SendEventAsync(msg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
                _timer.Cancel();
                _deferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (_deferral != null)
            {
                _deferral.Complete();
                _deferral = null;
            }
        }
    }
}
