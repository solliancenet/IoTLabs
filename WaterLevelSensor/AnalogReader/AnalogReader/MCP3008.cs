using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace AnalogReader
{
    /// <summary>
    /// Helper class that assists in reading from MCP3008 channels
    /// using SPI0, Mode0 and Standard Configuration
    /// assuming 3v3 
    /// </summary>
    public class MCP3008
    {
        private SpiDevice _mcp3008 = null;

        /// <summary>
        /// Connects to the MCP3008 on SPI0
        /// </summary>
        /// <returns>True if successful, False otherwise</returns>
        public async Task<bool> Connect()
        {
            var spiSettings = new SpiConnectionSettings(0);//for spi bus index 0
            spiSettings.ClockFrequency = 3600000; //3.6 MHz
            spiSettings.Mode = SpiMode.Mode0;

            string spiQuery = SpiDevice.GetDeviceSelector("SPI0");
            //using Windows.Devices.Enumeration;
            var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery);
            if (deviceInfo != null && deviceInfo.Count > 0)
            {
                _mcp3008 = await SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings);
                return true;
            }
            else
            {
                return false;
            }

        }

        public int SampleVoltage(byte adcChannel)
        {
            return _read(0);
        }

        /// <summary>
        /// Obtains a single sample reading on the specified channel
        /// </summary>
        /// <param name="channel">Channel to read</param>
        /// <returns>Analog Reading Value</returns>
        private int _read(byte channel)
        {
            //From data sheet -- 1 byte selector for channel 0 on the ADC
            //First Byte sends the Start bit for SPI
            //Second Byte is the Configuration Bit
            //1 - single ended 
            //0 - d2
            //0 - d1
            //0 - d0
            //             S321XXXX <-- single-ended channel selection configure bits
            // Channel 0 = 10000000 = 0x80 OR (8+channel) << 4
            int config = (8 + channel) << 4;
            var transmitBuffer = new byte[3] { 1, (byte)config, 0x00 };
            var receiveBuffer = new byte[3];

            _mcp3008.TransferFullDuplex(transmitBuffer, receiveBuffer);
            //first byte returned is 0 (00000000), 
            //second byte returned we are only interested in the last 2 bits 00000011 (mask of &3) 
            //then shift result 8 bits to make room for the data from the 3rd byte (makes 10 bits total)
            //third byte, need all bits, simply add it to the above result 
            var result = ((receiveBuffer[1] & 3) << 8) + receiveBuffer[2];
            return result;
        }

    

    }
}