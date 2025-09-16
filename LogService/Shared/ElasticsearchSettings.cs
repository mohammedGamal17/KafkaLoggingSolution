namespace LogService.Shared
{
    public class ElasticsearchSettings
    {
        public string Uri { get; set; } = "http://localhost:9200";
        public string Index { get; set; } = "logs-index";
    }
}
