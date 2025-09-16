namespace LogService.Shared
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "log-consumer-group";
        public string Topic { get; set; } = "logs";
    }
}
