using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace SampleService1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OrdersController(ILogger<OrdersController> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpGet()]
        public async Task<IActionResult> CreateOrder()
        {
            var log = new LogObj
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ServiceCode = "ORD",
                ServiceName = "OrderService",
                LogLevel = LoggingLevel.Error.ToSt(),
                ErrorCode = "ORD-001",
                Description = "Order failed due to timeout",
                RelatedCorrelationIds = new List<string> { "RELATED-123", "RELATED-456" },
                CreatedDate = DateTime.Now,
                Data = "{ OrderId: 123 }"
            };

            _logger.LogError(JsonSerializer.Serialize(log));
            //await _logger.SharedLogging(_serviceProvider ,log);
            return Ok("Order logged to Kafka");
        }
    }
}
