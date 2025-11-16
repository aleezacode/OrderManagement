using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using OrderManagement.Configuration;
using OrderManagement.Handlers.Order;
using OrderManagement.Kafka;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    public class BaseIntegrationTest : IAsyncLifetime
    {
        protected IServiceProvider ServiceProvider { get; private set; } = default!;
        protected IConfiguration Configuration { get; private set; } = default!;
        protected IMongoDatabase MongoDatabase { get; private set; } = default!;
        protected Mock<IEventProducer> MockEventProducer { get; private set; } = default!;
        protected IMediator Mediator => GetService<IMediator>();

        private IMongoClient _mongoClient = default!;
        private string _dbName = string.Empty;
        public async Task DisposeAsync()
        {
            foreach (var collectionName in _collectionsToDrop)
            {
                await MongoDatabase.DropCollectionAsync(collectionName);
            }
        }

        public virtual async Task InitializeAsync()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false)
                .Build();

            var connectionString = Environment.GetEnvironmentVariable("MongoDBSettings__ConnectionString") ?? Configuration["MongoDBSettings:ConnectionString"];
            _dbName = Configuration["MongoDBSettings:DatabaseName"]!;
            _mongoClient = new MongoClient(connectionString);
            MongoDatabase = _mongoClient.GetDatabase(_dbName);

            MockEventProducer = new Mock<IEventProducer>();
            _ = MockEventProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);

            var services = new ServiceCollection();

            services.Configure<MongoDBSettings>(Configuration.GetSection("MongoDBSettings"));

            services.AddSingleton(Configuration);
            services.AddSingleton<IMongoClient>(_mongoClient);
            services.AddSingleton(MongoDatabase);

            services.AddSingleton<IEventProducer>(MockEventProducer.Object);

            services.AddScoped<IRepository<Order>, OrderRepository>();
            services.AddScoped<IRepository<Inventory>, InventoryRepository>();
            services.AddScoped<IRepository<Payment>, PaymentRepository>();
            services.AddScoped<IRepository<Notification>, NotificationRepository>();
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IRepository<EventPublishlog>, EventPublishlogRepository>();

            services.AddLogging();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(PlaceOrderHandler).Assembly);
            });

            ServiceProvider = services.BuildServiceProvider();
        }

        protected async Task<User> GetTestUser()
        {
            var collection = MongoDatabase.GetCollection<User>("Users");
            var testUser = await collection.Find(x => x.Email == "test@mail.com").FirstOrDefaultAsync();

            if (testUser == null)
            {
                var newUser = new User()
                {
                    Email = "test@mail.com",
                    FullName = "Test User",
                    NotificationType = Type.Email
                };

                await collection.InsertOneAsync(newUser);
                return newUser;
            }

            return testUser;
        }

        protected async Task<Product> GetTestProduct()
        {
            var productCollection = MongoDatabase.GetCollection<Product>("Products");
            var inventoryCollection = MongoDatabase.GetCollection<Inventory>("Inventory");
            var testProduct = await productCollection.Find(x => x.Name == "TestProduct").FirstOrDefaultAsync();

            if (testProduct == null)
            {
                var newProduct = new Product()
                {
                    Name = "TestProduct",
                    Price = 29.99M
                };

               await productCollection.InsertOneAsync(newProduct);

                var inventory = new Inventory()
                {
                    ProductId = newProduct.Id,
                    Quantity = 100
                };
                await inventoryCollection.InsertOneAsync(inventory);

                return newProduct;
            }

            return testProduct;
        }

        protected async Task<Order> CreateTestOrder()
        {
            var collection = MongoDatabase.GetCollection<Order>("Orders");
            var product = await GetTestProduct();
            var user = await GetTestUser();

            var item = new OrderItem
            {
                ProductId = product.Id,
                UnitPrice = product.Price,
                Quantity = 2
            };

            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = item.UnitPrice * item.Quantity,
                OrderStatus = OrderStatus.Placed,
                Item = item
            };

            await collection.InsertOneAsync(order);

            return order;
        }

        private readonly string[] _collectionsToDrop = new[]
        {
            "Users",
            "Products",
            "Inventory",
            "Orders",
            "Payments",
            "Notifications",
            "EventPublishLogs"
        };


        protected T GetService<T>() where T : notnull
            => ServiceProvider.GetRequiredService<T>();
    }
}