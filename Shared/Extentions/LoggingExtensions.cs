using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Kafka;

namespace Shared
{
    public static class LoggingExtensions
    {
        public static void AddSharedLogging(this IServiceCollection services, IConfiguration configuration)
        {

            //services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
           // services.AddSingleton<KafkaLogProducer>();
            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
            var topic = configuration["Kafka:Topic"] ?? "logs";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter())
                .WriteTo.File(
                new JsonFormatter(), $"logs/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}/Log.json",
                restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.Kafka(
                    bootstrapServers: bootstrapServers,
                    topic: topic
                )
                .CreateLogger();


            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

        }
    }
}
