using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Kafka.Messages;
using Shared.Kafka.Services.Producer.Base;
namespace SampleService2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKafkaProducerService<PaymentStatus> _paymenStatus;

        public PaymentsController(ILogger<PaymentsController> logger, IServiceProvider serviceProvider, IKafkaProducerService<PaymentStatus> paymenStatus)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _paymenStatus = paymenStatus;
        }

        [HttpGet()]
        public async Task<IActionResult> CreatePayment()
        {
            var log = new LogObj
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ServiceCode = "PAY",
                ServiceName = "PaymentService",
                LogLevel = LoggingLevel.Info.ToString(),
                ErrorCode = "PAY-001",
                Description = "Payment success.",
                Data = new { Payment = 1999 }
            };

            _logger.LogInformation(JsonSerializer.Serialize(log));
            //await _logger.SharedLogging(_serviceProvider, log);
            await _paymenStatus.ProduceBatchAsync(new List<PaymentStatus>
            {
                new PaymentStatus
                {
                    OrderId = 123,
                    Status = "Paid"
                },
                new PaymentStatus
                {
                    OrderId = 124,
                    Status = "Paid"
                },
                new PaymentStatus
                {
                    OrderId = 125,
                    Status = "Paid"
                }
            }, "order-payments");

            return Ok("Payment logged to Kafka");
        }
    }
}
