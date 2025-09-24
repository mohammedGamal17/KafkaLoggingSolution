namespace Shared.Kafka.Messages
{
    public interface BaseKafkaMessage
    {
        public string MessageId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MessageType { get; set; }
    }
}
