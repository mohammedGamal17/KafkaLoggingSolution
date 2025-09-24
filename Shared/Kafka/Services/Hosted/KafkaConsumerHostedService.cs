namespace Shared.Kafka.Services.Consumer
{
    public class KafkaConsumerHostedService<TMessage> : IHostedService
        where TMessage : BaseKafkaMessage
    {
        #region Failds
        private readonly IKafkaConsumerService<TMessage> _consumerService;
        private readonly ILogger<KafkaConsumerHostedService<TMessage>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _topic;
        private readonly string _groupId;
        #endregion

        #region Constructors
        public KafkaConsumerHostedService(
            IKafkaConsumerService<TMessage> consumerService,
            ILogger<KafkaConsumerHostedService<TMessage>> logger,
            IServiceProvider serviceProvider,
            string topic,
            string groupId)
        {
            _consumerService = consumerService;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _topic = topic;
            _groupId = groupId;
        }
        #endregion

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _consumerService.OnMessageReceived += async (sender, message) =>
            {
                await HandleMessageAsync(message);
            };

            _consumerService.OnConsumptionError += (sender, exception) =>
            {
                _logger.LogError(exception, "Error in consumption for topic {Topic}", _topic);
            };

            await _consumerService.StartConsumingAsync(_topic, _groupId, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _consumerService.StopConsumingAsync();
        }

        private async Task HandleMessageAsync(TMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var handlerFactory = scope.ServiceProvider.GetRequiredService<IKafkaMessageHandlerFactory>();
            try
            {
                var handlers = scope.ServiceProvider.GetRequiredService<IEnumerable<IKafkaMessageHandler<TMessage>>>();

                foreach (var handler in handlers)
                {
                    try
                    {
                        var success = await handler.HandleAsync(message);
                        if (!success)
                        {
                            _logger.LogWarning("Handler {HandlerType} failed to process message {MessageId}",
                                handler.GetType().Name, message.MessageId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Handler {HandlerType} threw exception processing message {MessageId}",
                            handler.GetType().Name, message.MessageId);
                    }
                }

                _logger.LogDebug("Processed message {MessageId} with {HandlerCount} handlers",
                    message.MessageId, handlers.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message {MessageId}", message.MessageId);
            }
        }
    }
}
