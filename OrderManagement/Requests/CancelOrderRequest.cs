using System;

namespace OrderManagement.Requests
{
    public class CancelOrderRequest
    {
        public string OrderId { get; set; }
        public string Reason { get; set; }
    }
}