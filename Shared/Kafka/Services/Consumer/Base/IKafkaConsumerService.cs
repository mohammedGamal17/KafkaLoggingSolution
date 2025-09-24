namespace Shared.Kafka.Services.Consumer.Base
{
    public interface IKafkaConsumerService<TMessage> where TMessage : BaseKafkaMessage
    {
        Task StartConsumingAsync(string topic, string groupId, CancellationToken cancellationToken = default);
        Task StopConsumingAsync();
        event EventHandler<TMessage> OnMessageReceived;
        event EventHandler<Exception> OnConsumptionError;
    }
}
