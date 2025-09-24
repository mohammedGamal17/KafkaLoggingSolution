using Confluent.Kafka;

namespace Shared.Kafka.Services.Producer.Base
{
    public class KafkaProducerService<TMessage> : IKafkaProducerService<TMessage>, IDisposable
        where TMessage : BaseKafkaMessage
    {
        #region Failds
        private readonly IProducer<string, string> _producer;
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaProducerService<TMessage>> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        #endregion

        #region Constructor
        public KafkaProducerService(IOptions<KafkaSettings> settings, ILogger<KafkaProducerService<TMessage>> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                MessageTimeoutMs = _settings.MessageTimeoutMs,
                RetryBackoffMs = _settings.RetryBackoffMs,
                EnableIdempotence = true,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                    _logger.LogError("Producer error: {Reason}", error.Reason))
                .Build();
        }
        #endregion

        public async Task<bool> ProduceAsync(TMessage message, string? topic = null)
        {
            var targetTopic = topic ?? _settings.DefaultTopic;

            try
            {
                var jsonMessage = JsonSerializer.Serialize(message, _jsonOptions);
                var kafkaMessage = new Message<string, string>
                {
                    Key = message.MessageId,
                    Value = jsonMessage,
                    Timestamp = new Timestamp(message.CreatedDate)
                };

                var deliveryResult = await _producer.ProduceAsync(targetTopic, kafkaMessage);

                _logger.LogDebug("Message {MessageId} delivered to {Topic} [{Partition}] at offset {Offset}",
                    message.MessageId, deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);

                return true;
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to deliver message {MessageId} to Kafka: {Reason}",
                    message.MessageId, ex.Error.Reason);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error producing message {MessageId}", message.MessageId);
                return false;
            }
        }

        public async Task<bool> ProduceBatchAsync(IEnumerable<TMessage> messages, string? topic = null)
        {
            var targetTopic = topic ?? _settings.DefaultTopic;
            var successful = true;

            foreach (var message in messages)
            {
                var result = await ProduceAsync(message, targetTopic);
                if (!result) successful = false;
            }

            return successful;
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
