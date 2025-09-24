namespace Shared.Kafka.Handlers.Base
{
    public interface IKafkaMessageHandler<TMessage> where TMessage : BaseKafkaMessage
    {
        Task<bool> HandleAsync(TMessage message);
    }
}
