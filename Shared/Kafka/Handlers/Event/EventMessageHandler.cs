namespace Shared.Kafka.Handlers
{
    public class EventMessageHandler : IKafkaMessageHandler<EventMessage>
    {
        private readonly ILogger<EventMessageHandler> _logger;

        public EventMessageHandler(ILogger<EventMessageHandler> logger)
        {
            _logger = logger;
        }


        public Task<bool> HandleAsync(EventMessage message)
        {
            var data = message.EventData;
            _logger.LogInformation("Handling event: {EventName} with data: {EventData}", message.EventName, data);
            // Implement your event handling logic here
            _logger.LogInformation("Event {EventName} processed successfully.", message.EventName);
            return Task.FromResult(true);
        }
    }
}
