using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Shared
{
    public class KafkaLogProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaLogProducer(IOptions<KafkaOptions> options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _topic = options.Value.Topic;
        }
        public async Task LoggingAsync(LogObj log)
        {
            var message = new Message<string, string>
            {
                Key = log.CorrelationId,
                Value = JsonSerializer.Serialize(log)
            };
            await _producer.ProduceAsync(_topic, message);
        }
        public void Dispose()
        {
            _producer.Flush();
            _producer.Dispose();
        }
    }
}
