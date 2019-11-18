﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Polly;

[assembly: FunctionsStartup(typeof(WaterSensorPredictionFunction.Startup))]

namespace WaterSensorPredictionFunction
{
    public class Startup : FunctionsStartup
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add a new HttpClientFactory that can be injected into the functions.
            // We add resilience and transient fault-handling capabilities to the HttpClient instances that the factory creates
            // by adding a Polly Retry policy with a very brief back-off starting at quarter-of-a-second to two seconds.
            // We want the HTTP requests that are sent to the downstream Logic App service to wait before attempting to try
            // sending the message, giving it some "breathing room" in case the service is overwhelmed. We chose to make
            // the time between retries relatively brief so as not to disrupt Cosmos DB message processing for too long, but
            // enough time to hopefully allow the downstream service to recover.
            // See the following for more information:
            // https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly
            builder.Services.AddHttpClient(NamedHttpClients.LogicAppClient, client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(1000),
                TimeSpan.FromMilliseconds(2000)
            }));

            // Add a new HttpClientFactory with an async retry policy for communicating with the deployed ML scoring service.
            builder.Services.AddHttpClient(NamedHttpClients.ScoringServiceClient, client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromMilliseconds(50),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(1000)
            }));

            // Configure Application Insights so we can use it for custom logging.
            // The configuration object will be injected into the function(s).
            builder.Services.AddSingleton<TelemetryConfiguration>(sp =>
            {
                var telemetryConfiguration = new TelemetryConfiguration
                {
                    InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY")
                };
                telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
                return telemetryConfiguration;
            });
        }
    }
}
