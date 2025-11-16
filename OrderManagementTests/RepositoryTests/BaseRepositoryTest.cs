using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderManagement.Configuration;
using Testcontainers.MongoDb;

public abstract class BaseRepositoryTest : IAsyncLifetime
{
    protected IOptions<MongoDBSettings> MongoDbSettings { get; private set; } = default!;
    protected IMongoClient Client { get; private set; } = default!;
    protected IMongoDatabase Database { get; private set; } = default!;

    private readonly MongoDbContainer _mongoContainer;
    private readonly string _databaseName = "UnitTestDb";

    protected BaseRepositoryTest(string collectionName)
    {
        // Set defaults (collection names)
        MongoDbSettings = Options.Create(new MongoDBSettings
        {
            ConnectionString = "",
            DatabaseName = _databaseName,
            OrdersCollectionName = collectionName,
            InventoryCollectionName = collectionName,
            PaymentsCollectionName = collectionName,
            NotificationsCollectionName = collectionName,
            ProductsCollectionName = collectionName,
            UsersCollectionName = collectionName,
            EventPublishLogCollectionName = collectionName
        });

        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7")
            .Build();
    }

    public virtual async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        var conn = _mongoContainer.GetConnectionString();

        MongoDbSettings.Value.ConnectionString = conn;

        Client = new MongoClient(conn);
        Database = Client.GetDatabase(_databaseName);
    }

    public Task DisposeAsync() => _mongoContainer.DisposeAsync().AsTask();
}
