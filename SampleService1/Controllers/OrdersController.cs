using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Kafka.Messages;
using Shared.Kafka.Services.Producer.Base;

namespace SampleService1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        private readonly IKafkaProducerService<EventMessage> _eventProducer;
        private readonly IConfiguration _config;
        private readonly string _eventTopic;
        public OrdersController(ILogger<OrdersController> logger, IKafkaProducerService<EventMessage> eventProducer, IConfiguration config)
        {
            _logger = logger;
            _eventProducer = eventProducer;
            _config = config;
            _eventTopic = config["Kafka:Topics:events:order"] ?? "order-events";
        }

        [HttpGet()]
        public async Task<IActionResult> CreateOrder()
        {
            var log = new LogObj
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ServiceCode = "ORD",
                ServiceName = "OrderService",
                LogLevel = LoggingLevel.Warning.ToString(),
                ErrorCode = "ORD-001",
                Description = "Order failed due to timeout",
                RelatedCorrelationIds = new List<string> { "RELATED-123", "RELATED-456" },
                Data = "{ OrderId: 123 }"
            };

            _logger.LogError(JsonSerializer.Serialize(log));
            var eventMessage = new EventMessage
            {
                EventName = "OrderCreated",
                EventData = JsonSerializer.Serialize(new { OrderId = 123, Status = "Created" })
            };
            await _eventProducer.ProduceAsync(eventMessage, _eventTopic);
            return Ok("Order logged to Kafka");
        }
    }
}
