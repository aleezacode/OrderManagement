using MongoDB.Driver;
using OrderManagement.Configuration;
using MediatR;
using OrderManagement.Repositories;
using OrderManagement.Models;
using OrderManagement.Kafka;
using OrderManagement.Consumers.Order;
using OrderManagement.Consumers.Inventory;
using FluentValidation;
using OrderManagement.Validators;

var builder = WebApplication.CreateBuilder(args);

// Core framework services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Mongo setup
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
    return client.GetDatabase(settings.DatabaseName);
});

// Repositories & producer
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddScoped<IRepository<Inventory>, InventoryRepository>();
builder.Services.AddScoped<IRepository<Payment>, PaymentRepository>();
builder.Services.AddScoped<IRepository<Notification>, NotificationRepository>();
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IRepository<EventPublishlog>, EventPublishlogRepository>();
builder.Services.AddSingleton<IEventProducer, KafkaEventProducer>();

// Kafka consumers
builder.Services.AddHostedService<OrderPlacedConsumer>();
builder.Services.AddHostedService<InventoryReservedConsumer>();
builder.Services.AddHostedService<OrderCancelledConsumer>();
builder.Services.AddHostedService<InventoryShortageConsumer>();

Console.WriteLine("Before build");
var app = builder.Build();
Console.WriteLine("After Build");

// Mongo seeding
using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("Before seeding");
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    await DatabaseSeeder.SeedAsync(db);
    Console.WriteLine("After seeding");
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderManagement API V1");
    c.RoutePrefix = string.Empty;
});

// Middleware order
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Before App run");
await app.RunAsync();
