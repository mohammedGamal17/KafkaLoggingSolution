namespace Shared.Kafka.Messages
{
    public class PaymentStatus : BaseKafkaMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string MessageType { get; set; } = nameof(PaymentStatus);

        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
