using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Configuration
{
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string OrdersCollectionName { get; set; } = "Orders";
        public string InventoryCollectionName { get; set; } = "Inventory";
        public string PaymentsCollectionName { get; set; } = "Payments";
        public string NotificationsCollectionName { get; set; } = "Notifications";
        public string ProductsCollectionName { get; set; } = "Products";
        public string UsersCollectionName { get; set; } = "Users";
    }
}