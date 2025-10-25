using MongoDB.Bson;
using MongoDB.Driver;
using OrderManagement.Models;
public static class DatabaseSeeder
{
    private static readonly string KeyboardId = ObjectId.GenerateNewId().ToString();
    private static readonly string MouseId = ObjectId.GenerateNewId().ToString();
    private static readonly string MonitorId = ObjectId.GenerateNewId().ToString();
    public static async Task SeedAsync(IMongoDatabase database)
    {
        await SeedProductsAsync(database);
        await SeedInventoryAsync(database);
        await SeedUsersAsync(database);
    }

    private static async Task SeedProductsAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Product>("Products");
        var existingProducts = await collection.Find(_ => true).ToListAsync();
        if (existingProducts.Count > 0)
        {
            return; // Products already seeded
        }

        var count = await collection.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var products = new List<Product>
            {
                new() { Id = KeyboardId, Name = "Keyboard", Price = 49.99M },
                new() { Id = MouseId, Name = "Mouse", Price = 29.99M },
                new() { Id = MonitorId, Name = "Monitor", Price = 199.99M, }
            };

            await collection.InsertManyAsync(products);
            Console.WriteLine("✅ Seeded Products collection");
        }
    }

    private static async Task SeedInventoryAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<Inventory>("Inventory");
        var existingInventory = await collection.Find(_ => true).ToListAsync();
        if (existingInventory.Count > 0)
        {
            return; // Inventory already seeded
        }

        var count = await collection.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var inventory = new List<Inventory>
            {
                new() { ProductId = KeyboardId, Quantity = 50 },
                new() { ProductId = MouseId, Quantity = 50 },
                new() { ProductId = MonitorId, Quantity = 50 }
            };

            await collection.InsertManyAsync(inventory);
            Console.WriteLine("✅ Seeded Inventory collection");
        }
    }

    private static async Task SeedUsersAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<User>("Users");
        var existingUsers = await collection.Find(_ => true).ToListAsync();
        if (existingUsers.Count > 0)
        {
            return; // Users already seeded
        }

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
