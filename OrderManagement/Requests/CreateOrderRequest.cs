using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Requests
{
    public class CreateOrderRequest
    {
        public string userId { get; set; }
        public List<OrderItemRequest> items { get; set; }
    }

    public class OrderItemRequest
    {
        public string productId { get; set; }
        public int quantity { get; set; }
        public decimal unitPrice { get; set; }
    }
}