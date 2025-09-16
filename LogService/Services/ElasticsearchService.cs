using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using LogService.Shared;

namespace LogService.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public ElasticsearchService(ElasticsearchSettings settings)
        {
            _indexName = settings.Index;

            var pool = new SingleNodePool(new Uri(settings.Uri));
            var config = new ElasticsearchClientSettings(pool)
                .DefaultIndex(_indexName);

            _client = new ElasticsearchClient(config);
        }

        public async Task IndexLogAsync(LoggingMessage log, CancellationToken cancellationToken = default)
        {
            var response = await _client.IndexAsync(log, cancellationToken: cancellationToken);

            if (!response.IsValidResponse)
            {
                Console.Error.WriteLine($"Failed to index log: {response.ElasticsearchServerError}");
            }
        }

        public async Task<List<LoggingMessage>> GetAllLogsAsync(CancellationToken cancellationToken = default)
        {
            var searchResponse = await _client.SearchAsync<LoggingMessage>(s => s
                .Index(_indexName)
                .Size(1000), cancellationToken);

            return searchResponse.Documents.ToList();
        }

        public async Task<List<LoggingMessage>> SearchLogsAsync(string keyword, CancellationToken cancellationToken = default)
        {
            var searchResponse = await _client.SearchAsync<LoggingMessage>(s => s
                .Index(_indexName)
                .Query(q => q.Match(m => m.Field(f => f.Description).Query(keyword)))
                , cancellationToken);

            return searchResponse.Documents.ToList();
        }
    }
}
