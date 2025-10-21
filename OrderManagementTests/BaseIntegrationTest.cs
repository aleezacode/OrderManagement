using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderManagement.Configuration;

namespace OrderManagementTests;

public class BaseIntegrationTest
{
    protected readonly IMongoDatabase mongoDatabase;
    protected readonly IMongoClient mongoClient;
    protected readonly MongoDBSettings mongoDBSettings;

    protected BaseIntegrationTest()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json")
            .Build();

        mongoDBSettings = new MongoDBSettings();
        configuration.GetSection("MongoDBSettings").Bind(mongoDBSettings);

        mongoClient = new MongoClient(mongoDBSettings.ConnectionString);
        mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.DatabaseName);
    }

    protected IOptions<MongoDBSettings> GetMongoDBSettingsOptions()
    {
        var options = Options.Create(mongoDBSettings);
        return options;
    }
}
