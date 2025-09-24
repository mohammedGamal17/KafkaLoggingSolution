namespace Shared.Kafka.Factory
{
    public class KafkaMessageHandlerFactory : IKafkaMessageHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public KafkaMessageHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IKafkaMessageHandler<TMessage>? GetHandler<TMessage>() where TMessage : BaseKafkaMessage
        {
            return _serviceProvider.GetService<IKafkaMessageHandler<TMessage>>();
        }

        public IEnumerable<IKafkaMessageHandler<TMessage>> GetHandlers<TMessage>() where TMessage : BaseKafkaMessage
        {
            return _serviceProvider.GetServices<IKafkaMessageHandler<TMessage>>();
        }
    }
}
