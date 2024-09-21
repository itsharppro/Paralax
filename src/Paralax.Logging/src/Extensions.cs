using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.Grafana.Loki;
using Paralax.Logging.Options;
using Paralax.Core;
using Microsoft.AspNetCore.Routing;

namespace Paralax.Logging
{
    public static class Extensions
    {
        private const string LoggerSectionName = "logger";
        private const string AppSectionName = "app";
        internal static LoggingLevelSwitch LoggingLevelSwitch = new();

        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder,
            Action<HostBuilderContext, LoggerConfiguration> configure = null, 
            string loggerSectionName = LoggerSectionName,
            string appSectionName = AppSectionName)
        {
            return hostBuilder.ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
                .UseSerilog((context, loggerConfiguration) =>
                {
                    var loggerOptions = context.Configuration.GetOptions<LoggerOptions>(loggerSectionName);
                    var appOptions = context.Configuration.GetOptions<AppOptions>(appSectionName);
                    MapOptions(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                    configure?.Invoke(context, loggerConfiguration);
                });
        }

        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder,
            Action<WebHostBuilderContext, LoggerConfiguration> configure = null, 
            string loggerSectionName = LoggerSectionName,
            string appSectionName = AppSectionName)
        {
            return webHostBuilder.ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
                .UseSerilog((context, loggerConfiguration) =>
                {
                    var loggerOptions = context.Configuration.GetOptions<LoggerOptions>(loggerSectionName);
                    var appOptions = context.Configuration.GetOptions<AppOptions>(appSectionName);
                    MapOptions(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                    configure?.Invoke(context, loggerConfiguration);
                });
        }

        public static IEndpointConventionBuilder MapLogLevelHandler(this IEndpointRouteBuilder builder, 
            string endpointRoute = "~/logging/level")
        {
            return builder.MapPost(endpointRoute, async context =>
            {
                var service = context.RequestServices.GetService<ILoggingService>();
                if (service is null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("ILoggingService is not registered.");
                    return;
                }

                var level = context.Request.Query["level"].ToString();
                if (string.IsNullOrEmpty(level))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid value for logging level.");
                    return;
                }

                service.SetLoggingLevel(level);
                context.Response.StatusCode = StatusCodes.Status200OK;
            });
        }

        public static IParalaxBuilder AddCorrelationContextLogging(this IParalaxBuilder builder)
        {
            builder.Services.AddTransient<CorrelationContextLoggingMiddleware>();
            return builder;
        }

        public static IApplicationBuilder UseCorrelationContextLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationContextLoggingMiddleware>();
        }

        private static void MapOptions(LoggerOptions loggerOptions, AppOptions appOptions, 
            LoggerConfiguration loggerConfiguration, string environmentName)
        {
            LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(loggerOptions.Level);

            loggerConfiguration.Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.WithProperty("Application", appOptions.Service)
                .Enrich.WithProperty("Instance", appOptions.Instance)
                .Enrich.WithProperty("Version", appOptions.Version);

            foreach (var (key, value) in loggerOptions.Tags ?? new Dictionary<string, object>())
            {
                loggerConfiguration.Enrich.WithProperty(key, value);
            }

            foreach (var (key, value) in loggerOptions.MinimumLevelOverrides ?? new Dictionary<string, string>())
            {
                var logLevel = GetLogEventLevel(value);
                loggerConfiguration.MinimumLevel.Override(key, logLevel);
            }

            ConfigureLoggerSinks(loggerConfiguration, loggerOptions);
        }

        private static void ConfigureLoggerSinks(LoggerConfiguration loggerConfiguration, LoggerOptions options)
        {
            if (options.Console?.Enabled == true)
            {
                loggerConfiguration.WriteTo.Console();
            }

            if (options.File?.Enabled == true)
            {
                var path = string.IsNullOrWhiteSpace(options.File.Path) ? "logs/logs.txt" : options.File.Path;
                loggerConfiguration.WriteTo.File(path, rollingInterval: RollingInterval.Day);
            }

            if (options.Elk?.Enabled == true)
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.Elk.Url))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = options.Elk.IndexFormat ?? "logstash-{0:yyyy.MM.dd}"
                });
            }

            if (options.Seq?.Enabled == true)
            {
                loggerConfiguration.WriteTo.Seq(options.Seq.Url, apiKey: options.Seq.ApiKey);
            }

            if (options.Loki?.Enabled == true)
            {
                loggerConfiguration.WriteTo.GrafanaLoki(options.Loki.Url);
            }
        }

        internal static LogEventLevel GetLogEventLevel(string level)
        {
            return Enum.TryParse(level, true, out LogEventLevel logLevel)
                ? logLevel
                : LogEventLevel.Information;
        }
    }
}
