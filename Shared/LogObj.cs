namespace Shared
{
    public class LogObj
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string LogLevel { get; set; } = LoggingLevel.Info.ToString();
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public object Data { get; set; }
        public List<string> RelatedCorrelationIds { get; set; } = new();
    }
}
