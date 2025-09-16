namespace LogService.Shared
{


    public class SerilogObj
    {
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string MessageTemplate { get; set; }
        public string TraceId { get; set; }
        public string SpanId { get; set; }
        public Properties Properties { get; set; }
    }
    public class Properties
    {
        public string SourceContext { get; set; }
        public string ActionId { get; set; }
        public string ActionName { get; set; }
        public string RequestId { get; set; }
        public string RequestPath { get; set; }
        public string ConnectionId { get; set; }
    }
    public class LoggingMessage
    {
        public string CorrelationId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string LogLevel { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public object Data { get; set; }
        public List<string> RelatedCorrelationIds { get; set; } = new();
    }
}
