using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AnalogReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MCP3008 _adc = new MCP3008();
        private DispatcherTimer _timer = new DispatcherTimer();

        public MainPage()
        {
            this.InitializeComponent();
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
        private  void timer_Tick(object sender, object e)
        {
            int vol = _adc.SampleVoltage(0);
            txtStatus.Text = "READING: " + vol.ToString();
          
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
