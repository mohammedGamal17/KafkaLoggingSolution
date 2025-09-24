namespace Shared.Kafka.Factory
{
    public interface IKafkaMessageHandlerFactory
    {
        IKafkaMessageHandler<TMessage>? GetHandler<TMessage>() where TMessage : BaseKafkaMessage;
        IEnumerable<IKafkaMessageHandler<TMessage>> GetHandlers<TMessage>() where TMessage : BaseKafkaMessage;
    }
}
