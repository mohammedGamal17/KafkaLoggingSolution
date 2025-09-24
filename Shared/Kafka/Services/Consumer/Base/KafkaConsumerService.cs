using Confluent.Kafka;

namespace Shared.Kafka.Services.Consumer.Base
{
    public class KafkaConsumerService<TMessage> : IKafkaConsumerService<TMessage>, IDisposable
        where TMessage : BaseKafkaMessage
    {

        #region Failds
        private readonly IConsumer<string, string> _consumer;
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaConsumerService<TMessage>> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private Task? _consumingTask;
        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler<TMessage> OnMessageReceived;
        public event EventHandler<Exception> OnConsumptionError;
        #endregion

        #region Constructors
        public KafkaConsumerService(IOptions<KafkaSettings> settings, ILogger<KafkaConsumerService<TMessage>> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var config = new ConsumerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                GroupId = _settings.DefaultConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            _consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                    _logger.LogError("Consumer error: {Reason}", error.Reason))
                .Build();
        }
        #endregion

        public async Task StartConsumingAsync(string topic, string groupId, CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Update group ID if provided
            if (!string.IsNullOrEmpty(groupId))
            {
                _consumer.Subscribe(topic);
            }
            else
            {
                _consumer.Subscribe(topic);
            }

            _consumingTask = Task.Run(async () =>
            {
                _logger.LogInformation("Starting consumption of topic {Topic} with group {GroupId}",
                    topic, groupId);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_cancellationTokenSource.Token);

                        if (consumeResult?.Message?.Value != null)
                        {
                            await ProcessMessageAsync(consumeResult.Message.Value);
                            _consumer.StoreOffset(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
                        OnConsumptionError?.Invoke(this, ex);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in consumption loop");
                        OnConsumptionError?.Invoke(this, ex);
                    }
                }
            }, _cancellationTokenSource.Token);

            await Task.CompletedTask;
        }

        public async Task StopConsumingAsync()
        {
            _cancellationTokenSource?.Cancel();

            if (_consumingTask != null)
            {
                await _consumingTask;
            }

            _consumer.Close();
            _consumer.Unsubscribe();

            _logger.LogInformation("Stopped consumption");
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _consumer?.Dispose();
            _cancellationTokenSource?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task ProcessMessageAsync(string messageJson)
        {
            try
            {
                var message = JsonSerializer.Deserialize<TMessage>(messageJson, _jsonOptions);
                if (message != null)
                {
                    OnMessageReceived?.Invoke(this, message);
                    _logger.LogDebug("Received message {MessageId} of type {MessageType}",
                        message.MessageId, message.MessageType);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize message: {Message}", messageJson);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {Message}", messageJson);
                OnConsumptionError?.Invoke(this, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                OnConsumptionError?.Invoke(this, ex);
            }
            await Task.CompletedTask;
        }
    }
}
