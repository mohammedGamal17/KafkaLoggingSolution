namespace Shared.Kafka.Messages
{
    public class EventMessage : BaseKafkaMessage
    {
        public string MessageId { get ; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string MessageType { get; set; } = nameof(EventMessage);

        public string EventName { get; set; } = string.Empty;
        public string EventData { get; set; } = string.Empty;

    }
}
