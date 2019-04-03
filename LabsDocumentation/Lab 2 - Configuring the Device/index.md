# Configuring the Device
## Booting up for the first time
Using the USB to MicroUSB cable, connect the Raspberry Pi to your computer. The Raspberry Pi is only using this cable for 5v power.

The first boot may take some time as it is performing first time setup.

## Set Language and Network connectivity options
Once the device has booted for the first time, you must select a default language. Select your language of choice, then press the **Next** button

![Language Settings](./images/LanguageSettings.jpg)

## Optionally send analytics data to Microsoft
Windows IoT Core gives you the ability to share analytic information from the Operating System. This data allows Microsoft to improve the quality of the product. You may choose one of the options on the screen, or not choose any at all. When you've made your decision, press the **Next** button.

![Send Data To Microsoft](./images/PrivacyAnalytics.jpg)

## Location Information
Windows IoT Core also allows location information to be shared. If you have a requirement for the device to provide location information, ensure you set this value to **Yes**, for our purposes in this lab, it is fine to leave this value as **No**.

![Share location information](./images/PrivacySettings.jpg)

## Network Information
If you weren't able to setup the Wi-Fi information from the dashboard, you will be able to do it at this point in the setup process. Select the network with which you'd like to connect, then enter the Wi-Fi key to make the connection.

![Enter Wi-Fi Key](./images/EnterWiFiKey.jpg)

## Completed Setup
Once the device has been setup, the default application for Windows IoT Core will display.

![Windows IoT Core Dashboard](./images/IoTCoreDefaultApp.jpg)


