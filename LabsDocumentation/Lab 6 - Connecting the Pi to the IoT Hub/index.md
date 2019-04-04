# Connecting the Pi to the IoT Hub
## Provision the Device in IoT Hub
Open the IoT Dashboard and select *Connect to Azure*. Select the IoT Hub that you created. Next to the Device ID dropdown, press the *Create new device* link. Enter a good name for the Device and select your Pi from the Device To Provision list. Press the *Provision* button to provision the device. 

![IoT Dashboard Provisioning Tool](./images/IoTDashboardProvisionTool.png)

## Retrieve the Device Connection String
In the Azure Portal, open the IoT Hub, then under the *Explorers* section, select *IoT Devices*, then choose the device that was just created by the IoT Dashboard tool. Copy the Primary device connection string from this details page. 

![Portal Device Connection String](./images/PortalDeviceConnectionString.png)

