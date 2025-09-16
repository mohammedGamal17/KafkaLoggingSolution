using Elastic.Clients.Elasticsearch;
using LogService.Services;
using LogService.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddHostedService<KafkaConsumerService>();

//// Elasticsearch configuration
builder.Services.Configure<ElasticsearchSettings>(builder.Configuration.GetSection("Elasticsearch"));


// ---- Register Elasticsearch client ----
var elasticUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";

var settings = new ElasticsearchClientSettings(new Uri(elasticUri));

var elasticClient = new ElasticsearchClient(settings);
builder.Services.AddSingleton(elasticClient);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Kafka Log Consumer running...");

app.Run();
