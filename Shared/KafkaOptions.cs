namespace Shared
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:29092";
        public string Topic { get; set; } = "logs";
    }
}
