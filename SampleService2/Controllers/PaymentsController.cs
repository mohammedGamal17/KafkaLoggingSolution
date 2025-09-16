using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shared;
namespace SampleService2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PaymentsController(ILogger<PaymentsController> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpGet()]
        public async Task<IActionResult> CreatePayment()
        {
            var log = new LogObj
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ServiceCode = "PAY",
                ServiceName = "PaymentService",
                LogLevel = LoggingLevel.Success.ToSt(),
                ErrorCode = "PAY-001",
                Description = "Payment success.",
                CreatedDate = DateTime.UtcNow,
                Data = new { Payment = 1999 }
            };

            _logger.LogInformation( JsonSerializer.Serialize(log));
            //await _logger.SharedLogging(_serviceProvider, log);
            return Ok("Payment logged to Kafka");
        }
    }
}
