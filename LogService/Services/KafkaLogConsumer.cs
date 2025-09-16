using System.Text.Json;
using Confluent.Kafka;
using LogService.Shared;
using Microsoft.Extensions.Options;

namespace LogService.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly KafkaSettings _settings;

        private readonly ElasticsearchService _elasticsearch;
        private readonly JsonSerializerOptions _jsonOptions;

        public KafkaConsumerService(
            ILogger<KafkaConsumerService> logger,
            IOptions<KafkaSettings> settings
            )
        {
            _logger = logger;
            _settings = settings.Value;
            _elasticsearch = new ElasticsearchService(new ElasticsearchSettings
            {
                Uri = Environment.GetEnvironmentVariable("Elasticsearch__Uri") ?? "http://elasticsearch:9200",
                Index = Environment.GetEnvironmentVariable("Elasticsearch__Index") ?? "logs-index"
            });
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.GroupId,

                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_settings.Topic);

            _logger.LogInformation("Kafka consumer started. Listening to topic {Topic}", _settings.Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<Ignore, string>? result;
                    try
                    {
                        result = consumer.Consume(stoppingToken);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Consume error");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(result?.Message?.Value)) continue;
                    await ProcessMessageAsync(result.Message.Value, stoppingToken);

                    //try
                    //{

                    //    if (result?.Message?.Value != null)
                    //    {
                    //        try
                    //        {
                    //            var log = JsonSerializer.Deserialize<SerilogObj>(result.Message.Value);

                    //            try
                    //            {
                    //                var logobj = JsonSerializer.Deserialize<LoggingMessage>(log.MessageTemplate);
                    //                if (logobj != null)
                    //                {

                    //                    // Example: write to file
                    //                    await File.AppendAllTextAsync("logs/consumed-logs.json",
                    //                        JsonSerializer.Serialize(logobj) + Environment.NewLine,
                    //                        stoppingToken);

                    //                    await _elasticsearch.IndexLogAsync(logobj, stoppingToken);
                    //                }
                    //            }
                    //            catch (Exception)
                    //            {
                    //                await File.AppendAllTextAsync("logs/System-logs.json",
                    //                    JsonSerializer.Serialize(log) + Environment.NewLine,
                    //                    stoppingToken);
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            // Handle deserialization errors
                    //        }

                    //    }
                    //}
                    //catch (ConsumeException ex)
                    //{
                    //    _logger.LogError(ex, "Consume error");
                    //}
                
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer closing...");
            }
            finally
            {
                consumer.Close();
                consumer.Dispose();
                _logger.LogInformation("Kafka consumer closed.");
            }
        }


        private async Task ProcessMessageAsync(string message, CancellationToken token)
        {
            try
            {
                var serilogObj = JsonSerializer.Deserialize<SerilogObj>(message, _jsonOptions);
                if (serilogObj is null)
                    return;

                LoggingMessage? logObj = null;

                try
                {
                    logObj = JsonSerializer.Deserialize<LoggingMessage>(serilogObj.MessageTemplate, _jsonOptions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse LoggingMessage, saving fallback log.");
                    await SaveToFileAsync("logs/System-logs.json", serilogObj, token);
                }

                if (logObj is not null)
                {
                    await SaveToFileAsync("logs/consumed-logs.json", logObj, token);
                    await _elasticsearch.IndexLogAsync(logObj, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message: {Message}", message);
            }
        }
        private static async Task SaveToFileAsync<T>(string filePath, T obj, CancellationToken token)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(obj);
            await File.AppendAllTextAsync(filePath, json + Environment.NewLine, token);
        }
    }
}