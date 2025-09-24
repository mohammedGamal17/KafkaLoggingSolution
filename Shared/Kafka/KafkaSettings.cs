namespace Shared.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string DefaultTopic { get; set; } = "logs";
        public string DefaultConsumerGroup { get; set; } = "default-consumer-group";
        public int MessageTimeoutMs { get; set; } = 5000;
        public int RetryBackoffMs { get; set; } = 1000;
    }
}
