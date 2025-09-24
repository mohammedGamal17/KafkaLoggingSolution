using Shared.Kafka.Handlers;
using Shared.Kafka.Messages;

// SampleService2

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Kafka

#region Configure
//builder.Services.AddSharedLogging(builder.Configuration);
builder.Services.AddGenericKafkaServices(builder.Configuration);
#endregion

#region Producer
#endregion

#region Consumer
// 1. register consumer
var eventTopic = builder.Configuration["Kafka:Topics:events:order"] ?? "order-events";
var groupId = builder.Configuration["Kafka:GroupIds:sample-service-2-group"] ?? "sample-service-2-group";

builder.Services.AddKafkaConsumer<EventMessage>(topic: eventTopic, groupId: groupId);
// 2. register handler
builder.Services.AddKafkaMessageHandler<EventMessageHandler, EventMessage>();
#endregion

#endregion

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

app.Run();
