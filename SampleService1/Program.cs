using SampleService1.Services;
using Shared.Kafka.Messages;

// SampleService1

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


#region Kafka

#region Configure
//builder.Services.AddSharedLogging(builder.Configuration);
builder.Services.AddGenericKafkaServices(builder.Configuration);
#endregion

#region Producer

builder.Services.AddKafkaProducer<EventMessage>();
#endregion

#region Consumer
// 1. register consumer
builder.Services.AddKafkaConsumer<PaymentStatus>(topic: "order-payments", groupId: "sample-service-1-group");
// 2. register handler

builder.Services.AddKafkaMessageHandler<OrderPaymentStatusHandler, PaymentStatus>();
#endregion

#endregion


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

app.Run();
