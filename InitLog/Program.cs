using Confluent.Kafka;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:29092" // your Kafka broker
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        var log = new
        {
            CorrelationId = Guid.NewGuid().ToString(),
            ServiceName = "InitProducer",
            Description = "First log to create/open the topic",
            CreatedDate = DateTime.UtcNow
        };

        var message = JsonSerializer.Serialize(log);

        var topic = "logs"; // topic name

        try
        {
            var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });

            Console.WriteLine($"✅ Sent log to topic '{topic}' at offset {result.Offset}: {message}");
        }
        catch (ProduceException<Null, string> ex)
        {
            Console.WriteLine($"❌ Error producing message: {ex.Error.Reason}");
        }

        producer.Flush(TimeSpan.FromSeconds(2));
    }
}
