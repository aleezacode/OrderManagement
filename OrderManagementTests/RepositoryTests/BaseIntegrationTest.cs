using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using OrderManagement.Configuration;

namespace OrderManagementTests;

public class BaseIntegrationTest : IDisposable
{
    protected readonly MongoDbRunner _mongoRunner;
    protected readonly IOptions<MongoDBSettings> _mongoDBSettings;
    protected BaseIntegrationTest(string collectionName)
    {
        _mongoRunner = MongoDbRunner.Start();
        _mongoDBSettings = Options.Create(new MongoDBSettings
        {
            ConnectionString = _mongoRunner.ConnectionString,
            DatabaseName = "OrderManagementTestDb",
            OrdersCollectionName = collectionName
        });
    }

    public void Dispose()
    {
        _mongoRunner.Dispose();
    }
}
