using Shared.Kafka.Handlers.Base;
using Shared.Kafka.Messages;

namespace SampleService1.Services
{
    public class OrderPaymentStatusHandler : IKafkaMessageHandler<PaymentStatus>
    {
        private readonly ILogger<OrderPaymentStatusHandler> _logger;
        public OrderPaymentStatusHandler(ILogger<OrderPaymentStatusHandler> logger)
        {
            _logger = logger;
        }


        public async Task<bool> HandleAsync(PaymentStatus message)
        {
            _logger.LogInformation("Processing payment status for OrderId: {OrderId}, Status: {Status}", message.OrderId, message.Status);

            await Task.Delay(5000);

            _logger.LogInformation("Payment status for OrderId: {OrderId} processed successfully.", message.OrderId);


            return await Task.FromResult(true);
        }
    }
}
