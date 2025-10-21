using MongoDB.Bson;
using MongoDB.Driver;
using OrderManagement.Models;
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IMongoDatabase database)
    {
        await SeedProductsAsync(database);
        await SeedInventoryAsync(database);
        await SeedUsersAsync(database);
    }

    private static async Task SeedProductsAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Product>("Products");

        var count = await collection.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var products = new List<Product>
            {
                new() { Id = "E386D2223E5BAA35CC2D4BCC", Name = "Keyboard", Price = 49.99M },
                new() { Id = "DE3DD781D59DFFE74F38FEC8", Name = "Mouse", Price = 29.99M },
                new() { Id = "BA53B385220BDE8F9BC9E7C1", Name = "Monitor", Price = 199.99M, }
            };

            await collection.InsertManyAsync(products);
            Console.WriteLine("✅ Seeded Products collection");
        }
    }

    private static async Task SeedInventoryAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Inventory>("Inventory");

        var count = await collection.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var inventory = new List<Inventory>
            {
                new() { ProductId = "E386D2223E5BAA35CC2D4BCC", Quantity = 50 },
                new() { ProductId = "DE3DD781D59DFFE74F38FEC8", Quantity = 50 },
                new() { ProductId = "BA53B385220BDE8F9BC9E7C1", Quantity = 50 }
            };

            await collection.InsertManyAsync(inventory);
            Console.WriteLine("✅ Seeded Inventory collection");
        }
    }

    private static async Task SeedUsersAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<User>("Users");

        var count = await collection.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var users = new List<User>
            {
                new() { Email = "test1@example.com", FullName = "John Doe"},
                new() { Email = "test2@example.com", FullName = "Jane Smith" }
            };

            await collection.InsertManyAsync(users);
            Console.WriteLine("✅ Seeded Users collection");
        }
    }
}
