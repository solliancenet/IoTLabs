# Connecting the Pi to the IoT Hub
## Provision the Device in IoT Hub
Open the IoT Dashboard and select *Connect to Azure*. Select the IoT Hub that you created. Next to the Device ID dropdown, press the *Create new device* link. Enter a good name for the Device and select your Pi from the Device To Provision list. Press the *Provision* button to provision the device. ![IoT Dashboard Provisioning Tool](./images/IoTDashboardProvisionTool.png)

## Retrieve the Device Connection String
In the Azure Portal, open the IoT Hub, then under the *Explorers* section, select *IoT Devices*, then choose the device that was just created by the IoT Dashboard tool. Copy the Primary device connection string from this details page. ![Portal Device Connection String](./images/PortalDeviceConnectionString.png)

## Configure TPM Settings with the Device Connection string
Open the IoT Dashboard, on the *My Devices* section, right-click on your Pi device, and select the *Open in device portal* option. ![Open in device portal](./images/openindeviceportal.png)
In the device portal, select the *TPM Configuration* menu option. From there, paste your device connection string into the Azure Connection String textbox. ![Enter Device Connection String in TPM Configuration]
Upon completion of this step, reboot your Pi using the IoT Dashboard application (My Devices -> right click on device -> Restart)
![Configure Device Connection String](./images/EnterDeviceConnectionString.png)



