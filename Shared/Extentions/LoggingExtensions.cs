using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Kafka;

namespace Shared
{
    public static class LoggingExtensions
    {
        //public static IServiceCollection AddSharedLogging(this IServiceCollection services, IConfiguration configuration)
        //{
        //    // Bind Kafka options
        //    services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        //    // Register Kafka producer
        //    services.AddSingleton<KafkaLogProducer>();

        //    // Configure Serilog
        //    Log.Logger = new LoggerConfiguration()
        //        .ReadFrom.Configuration(configuration)

        //        .Enrich.FromLogContext()
        //        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        //        .WriteTo.File(
        //        new JsonFormatter(), $"logs/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}/Log.json",
        //        restrictedToMinimumLevel: LogEventLevel.Information)
        //        .CreateLogger();

        //    // Hook Serilog into Microsoft logging pipeline
        //    services.AddLogging(loggingBuilder =>
        //    {
        //        loggingBuilder.ClearProviders();
        //        loggingBuilder.AddSerilog(dispose: true);
        //    });

        //    return services;
        //}


        public static void AddSharedLogging(this IServiceCollection services, IConfiguration configuration)
        {

            //services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
           // services.AddSingleton<KafkaLogProducer>();
            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
            var topic = configuration["Kafka:Topic"] ?? "logs";

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(new JsonFormatter())
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
        public static async Task SharedLogging(this Microsoft.Extensions.Logging.ILogger logger, IServiceProvider serviceProvider, LogObj log)
        {
            var producer = serviceProvider.GetRequiredService<KafkaLogProducer>();
            await producer.LoggingAsync(log);
        }
    }
}
