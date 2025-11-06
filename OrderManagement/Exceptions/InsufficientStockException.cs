using System;

namespace OrderManagement.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public string ProductId { get; }
        public int RequestedQuantity { get; }
        public int AvailableQuantity { get; }

        public InsufficientStockException(string productId, int requestedQuantity, int availableQuantity)
            : base($"Insufficient stock for product with ID {productId}. Available quantity: {availableQuantity}, requested quantity: {requestedQuantity}")
        {
            ProductId = productId;
            RequestedQuantity = requestedQuantity;
            AvailableQuantity = availableQuantity;
        }
    }
}