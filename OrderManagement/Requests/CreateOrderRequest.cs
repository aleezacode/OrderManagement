using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Requests
{
    public class CreateOrderRequest
    {
        public string UserId { get; set; }
        public OrderItemRequest Item { get; set; }
    }

    public class OrderItemRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}