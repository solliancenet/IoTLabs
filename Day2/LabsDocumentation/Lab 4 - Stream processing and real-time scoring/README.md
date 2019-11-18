# Lab 4: Stream processing and real-time scoring

In this lab, you will build a stream processing pipeline to consume IoT events from IoT Hub, transform and aggregate the data, and send the prepared data to the deployed machine learning (ML) model for real-time scoring.

The project consists of a data generator console application that registers simulated IoT devices to IoT Hub and sends water sensor telemetry. It also includes an Azure Functions project that contains the event processing logic to prepare the data and send it to the deployed ML model to detect anomalies in real time. You can run the function locally, or choose to deploy it to Azure.

## Pre-requisites

- [Visual Studio 2019 community](https://visualstudio.microsoft.com/vs/community/)
- [Visual Studio **.NET Desktop Development** and **ASP.NET and Web Development** Workloads](https://docs.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2019)
- [.NET Core 3.0 **SDK**](https://dotnet.microsoft.com/download/dotnet-core/3.0)

## Configure the data generator and Azure Functions projects

1. Open the **Day2/SourceCode/WaterSensorPrediction/WaterSensorPrediction.sln** solution file in Visual Studio.

2. In Solution Explorer, expand the **WaterSensorPredictionGenerator** console project and open the **appsettings.json** file. Update the `IOT_HUB_CONNECTION_STRING` value by pasting the **Event Hub-compatible endpoint** value from your IoT Hub service.

   ![The appsettings.json file is displayed.](media/vs-app-settings.png 'Solution Explorer')

3. To find this value, navigate to the Azure portal (<https://portal.azure.com>), open your IoT Hub service, select **Shared access policies** in the left-hand menu, select the **iothubowner** policy, then copy the **Connection string--primary key** value.

   ![The IoT Hub shared access policies blade is displayed.](media/iot-hub-connection-string.png 'IoT Hub')

4. Save the file.

5. In Solution Explorer, expand the **WaterSensorPredictionFunction** Azure Functions project and open the **local.settings.json** file. Update the `IotHubConnectionString` value by pasting the **Event Hub-compatible endpoint** value from your IoT Hub service. Next, update the `ScoringServiceUrl` value by pasting the URL to your deployed ML model that is displayed at the bottom of the `anomaly_detection_lab3.ipynb` notebook. Finally, update the `APPINSIGHTS_INSTRUMENTATIONKEY` value with your Application Insights instrumentation key.

   ![The local.settings.json file is displayed.](media/vs-local-settings.png 'Solution Explorer')

6. To find the `IotHubConnectionString` value, navigate to the Azure portal (<https://portal.azure.com>), open your IoT Hub service, select **Built-in endpoints** in the left-hand menu, then copy the **Event Hub-compatible endpoint** value.

   ![The IoT Hub built-in endpoints blade is displayed.](media/iot-hub-endpoints.png 'IoT Hub')

7. To find the `APPINSIGHTS_INSTRUMENTATIONKEY` value, navigate to the Azure portal, open your lab Resource Group, then locate the Application Insights service that was automatically provisioned when you created the Azure Machine Learning service workspace. If you cannot find an Application Insights instance, create a new one. Within the Overview blade, find and copy the **Instrumentation Key** value.

   ![The instrumentation key is highlighted in the overview blade.](media/app-insights-instrumentation-key.png 'App Insights')
