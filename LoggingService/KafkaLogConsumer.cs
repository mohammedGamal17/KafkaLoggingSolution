using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using System.Text.Json;

namespace LoggingService
{
    public class KafkaLogConsumer : BackgroundService
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ConsumerConfig _config;
        private readonly string _topic = "logs";
        public KafkaLogConsumer(ElasticsearchClient elasticClient)
        {
            _elasticClient = elasticClient;
            _config = new ConsumerConfig
            {
                GroupId = "log-consumers",
                BootstrapServers = "localhost:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config).Build();
            consumer.Subscribe(_topic); consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    var logMessage = JsonSerializer.Deserialize<Shared.LoggingMessage>(consumeResult.Message.Value);
                    if (logMessage != null)
                    {
                        var response = await _elasticClient.IndexAsync(logMessage, idx => idx.Index("logs"), stoppingToken);
                        if (!response.IsValidResponse)
                        {
                            Console.WriteLine($"Failed to index log: {response.DebugInformation}");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
