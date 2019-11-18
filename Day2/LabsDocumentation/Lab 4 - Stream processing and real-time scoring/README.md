# Lab 4: Stream processing and real-time scoring

In this lab, you will build a stream processing pipeline to consume IoT events from IoT Hub, transform and aggregate the data, and send the prepared data to the deployed machine learning (ML) model for real-time scoring.

The project consists of a data generator console application that registers simulated IoT devices to IoT Hub and sends water sensor telemetry. It also includes an Azure Functions project that contains the event processing logic to prepare the data and send it to the deployed ML model to detect anomalies in real time. You can run the function locally, or choose to deploy it to Azure.

When the deployed ML model detects an anomaly in the sensor data, the function logs a custom event in Application Insights, named `Anomaly`. These custom events, along with all Azure Functions telemetry, are stored by Application Insights for analysis and logging. Alerts are created within Application Insights to notify admins when certain thresholds are met, such as _n_ anomalies within _x_ seconds.

![The service components are displayed within a high-level architecture diagram.](media/architecture-diagram.png 'Architecture diagram')

Application Insights displaying Anomaly events compared to requests to the Azure Functions instance:

![The App Insights interface is displayed.](media/app-insights.png 'Application Insights')

## Pre-requisites

- [Visual Studio 2019 community](https://visualstudio.microsoft.com/vs/community/)
- [Visual Studio **.NET Desktop Development**, **ASP.NET and Web Development**, and **Azure Development** Workloads](https://docs.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2019)
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

8. Save the file.

## Execute the data generator and run the Azure function locally

Now that the data generator and Azure Functions projects have been configured, it's time to run them locally.

1. In Visual Studio, right-click the **WaterSensorPredictionFunction** Azure Functions project, select **Debug**, then **Start new instance**.

   ![The Debug dialog is shown in Visual Studio.](media/vs-debug-function.png 'Debug')

2. In a moment, the Azure Functions runtime will launch in a new console window. **Let this continually run for the remainder of the lab.**

   ![The Azure Functions runtime is displayed.](media/functions-window.png 'Azure Functions Core Tools')

3. In Visual Studio, right-click the **WaterSensorPredictionGenerator** console project, select **Debug**, then **Start new instance**.

4. Open both console windows side-by-side and observe the outputs of each. After a brief period, depending on your computer and network speed, you will begin seeing messages output in the Azure Functions window in yellow, stating "_Anomaly detected within the following device: {deviceId}_". This occurs when the function sends the sensor data to the deployed ML model hosted in the Azure Container Instance (ACI) container, and the model sends back a prediction that an anomaly has occurred. Each of the blue lines in the output ("_Executed 'WaterLevelAnomalyFunction'..._") represent each time the function is triggered by sensor events flowing through IoT Hub.

   ![The data generator and Azure Functions console windows are displayed side-by-side.](media/console-windows.png 'Console windows')

5. Allow both of these windows to remain open in the background for the next task.

## Explore metrics and events in Application Insights and create alerts

Let's explore the metrics and events captured by Application Insights, then set up alerts.

1. Open the Azure portal (<https://portal.azure.com>), your lab resource group, then open your Application Insights instance.

2. Select **Events** in the left-hand menu.

3. At the top of the Events blade, set the following filtering options:

   - **Show occurrences for**: All Users.
   - **Who used**: Any Custom Event, Request or Page View.
   - **During**: Last hour.
   - **By**: 3 minutes.
   - **Split by**: DeviceId.

   > **Please note**: You may not see `DeviceId` as a split by option yet. It takes a few minutes for the custom properties to display.

   ![The event filters are displayed with the previously described values.](media/app-insights-events-filters.png 'Event filters')

   Filters applied:

   ![The App Insights interface is displayed.](media/app-insights.png 'Application Insights')

   At the bottom of the page, underneath the chart, you can see the `WaterLevelAnomalyFunction` requests (these are all requests to the Azure Function) compared to the number of reported `Anomaly` events:

   ![The Event Statistics compares the counts of each event type.](media/app-insights-event-statistics.png 'Event Statistics')

4. At the top of the chart, select the **Open chart in Logs (Analytics)** button.

   ![The button is highlighted.](media/app-insights-open-chart-in-logs.png 'Open chart in logs')

5. A query similar to the following will be displayed in Log Analytics:

   ```sql
   union requests,pageViews,customEvents
   | where timestamp between(datetime("2019-11-17T15:00:00.000Z")..datetime("2019-11-18T15:00:00.000Z"))
   | summarize Ocurrences=count() by tostring(customDimensions["DeviceId"]) , bin(timestamp, 1h)
   | order by timestamp asc
   | render barchart
   ```

   Modify the query to add another `where` clause to filter out events where the custom "DeviceId" has an empty value. Here is the updated script that adds the following line: `| where customDimensions["DeviceId"] != ''`:

   ```sql
   union requests,pageViews,customEvents
   | where timestamp between(datetime("2019-11-17T15:00:00.000Z")..datetime("2019-11-18T15:00:00.000Z"))
   | where customDimensions["DeviceId"] != ''
   | summarize Ocurrences=count() by tostring(customDimensions["DeviceId"]) , bin(timestamp, 1h)
   | order by timestamp asc
   | render barchart
   ```

   Finally, change the Chart type to **Doughnut**:

   ![The doughnut chart is displayed in log analytics.](media/log-analytics.png 'Log Analytics')

   This chart displays the number of anomalies (and percentages) detected for each device within the specified timeframe.

6. Go back to your Application Insights instance, then select **Metrics** in the left-hand menu.

7. Configure the filters for the metrics with the following parameters:

   - **Metric namespace**: Select `azure.applicationinsights` underneath the Custom header.
   - **Metric**: Select `Cluster1Anomalies`.
   - **Aggregation**: Select `Count`.

   ![The metrics filters are displayed as described.](media/app-insights-metrics-filters.png 'Metrics filters')

   Your chart should now look similar to the following:

   ![The metrics chart shows the number of Cluster1Anomalies instances.](media/app-insights-metrics.png 'Metrics')

8. Above the metrics chart you just created, select **New alert rule**.

   ![The new alert rule button is highlighted.](media/app-insights-new-alert-rule.png 'New alert rule button')

9. In the Create Rule blade, select the `Whenever the Cluster1Anomalies is <logic undefined>` condition. The Configure signal logic dialog will appear. Within this dialog, scroll down to the Alert logic section and configure the following parameters:

   - **Threshold**: Static.
   - **Operator**: Greater than.
   - **Aggregation type**: Count.
   - **Threshold value**: Enter 5.
   - **Aggregation granularity (Period)**: 1 minute.
   - **Frequency of evaluation**: Every 1 Minute.

   ![The create rule blade is displayed.](media/alerts-configure-signal-logic.png 'Configure signal logic')

10. Select **Done** to close the signal logic configuration dialog. Scroll down to the Alert Details section of the Create rule blade, then enter the following parameters:

    - **Alert rule name**: Cluster 1 device anomaly threshold exceeded.
    - **Description**: Water sensors within the Cluster 1 group have exceeded the anomaly threshold count.

    ![The alert details are completed as described.](media/alerts-alert-details.png 'Alert details')

11. Select **Create alert rule**.

> It is outside the scope of this lab to configure the alert audiences in Azure. To learn more about creating and configuring Azure Monitor metrics, see the following: <https://docs.microsoft.com/azure/azure-monitor/platform/alerts-metric-overview>.
