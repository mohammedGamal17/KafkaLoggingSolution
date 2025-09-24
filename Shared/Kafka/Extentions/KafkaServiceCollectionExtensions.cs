

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KafkaServiceCollectionExtensions
    {
        public static IServiceCollection AddGenericKafkaServices(this IServiceCollection services, IConfiguration configuration)
        {
            AddSharedLogging(services,configuration);

            services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

            // Register factory
            services.AddSingleton<IKafkaMessageHandlerFactory, KafkaMessageHandlerFactory>();

            return services;
        }
        public static IServiceCollection AddKafkaProducer<TMessage>(this IServiceCollection services)
            where TMessage : BaseKafkaMessage
        {
            services.AddScoped<IKafkaProducerService<TMessage>, KafkaProducerService<TMessage>>();
            return services;
        }

        public static IServiceCollection AddKafkaConsumer<TMessage>(this IServiceCollection services,
            string topic, string groupId)
            where TMessage : BaseKafkaMessage
        {
            services.AddSingleton<IKafkaConsumerService<TMessage>, KafkaConsumerService<TMessage>>();

            services.AddSingleton<IHostedService>(provider =>
                new KafkaConsumerHostedService<TMessage>(
                    provider.GetRequiredService<IKafkaConsumerService<TMessage>>(),
                    provider.GetRequiredService<ILogger<KafkaConsumerHostedService<TMessage>>>(),
                    provider,
                    topic,
                    groupId));

            return services;
        }
        public static IServiceCollection AddKafkaMessageHandler<THandler, TMessage>(this IServiceCollection services)
            where THandler : class, IKafkaMessageHandler<TMessage>
            where TMessage : BaseKafkaMessage
        {
            services.AddScoped<IKafkaMessageHandler<TMessage>, THandler>();
            return services;
        }
        private static void AddSharedLogging(IServiceCollection services, IConfiguration configuration)
        {

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
