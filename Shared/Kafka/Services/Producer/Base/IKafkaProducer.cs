namespace Shared.Kafka.Services.Producer.Base
{
    public interface IKafkaProducerService<TMessage> where TMessage : BaseKafkaMessage
    {
        Task<bool> ProduceAsync(TMessage message, string? topic = null);
        Task<bool> ProduceBatchAsync(IEnumerable<TMessage> messages, string? topic = null);
    }
}
