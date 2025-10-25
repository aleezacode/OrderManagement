using MongoDB.Driver;
using OrderManagement.Configuration;
using MediatR;
using OrderManagement.Repositories;
using OrderManagement.Models;
using OrderManagement.Kafka;
using OrderManagement.Consumers;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IRepository<Order>, OrderRepository>();
builder.Services.AddScoped<IRepository<Inventory>, InventoryRepository>();
builder.Services.AddScoped<IRepository<Payment>, PaymentRepository>();
builder.Services.AddScoped<IRepository<Notification>, NotificationRepository>();
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
builder.Services.AddSingleton<IEventProducer, KafkaEventProducer>();

// Register MediatR

//builder.Services.AddHostedService<OrderPlacedConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderManagement API V1");
    c.RoutePrefix = string.Empty;
});

//app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRouting();
app.MapControllers();

app.Run();