using MongoDB.Driver;
using Microsoft.Extensions.Options;
using OrderManagement.Configuration;
using Testcontainers.MongoDb;

namespace OrderManagementTests;

public class BaseRepositoryTest : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    protected IOptions<MongoDBSettings> MongoDbSettings { get; private set; } = default!;
    protected IMongoClient Client { get; private set; } = default!;
    protected IMongoDatabase Database { get; private set; } = default!;

    private readonly string _collectionName;

    protected BaseRepositoryTest(string collectionName)
    {
        _collectionName = collectionName;

        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var conn = _mongoContainer.GetConnectionString();
        MongoDbSettings = Options.Create(new MongoDBSettings
        {
            ConnectionString = conn,
            DatabaseName = "UnitTestDb",
            OrdersCollectionName = _collectionName
        });

        Client = new MongoClient(conn);
        Database = Client.GetDatabase("UnitTestDb");
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }
}
