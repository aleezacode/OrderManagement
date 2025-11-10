using System;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Handlers.Order;
using OrderManagement.Kafka;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests.HandlerTest
{
    public class PlaceOrderHandlerTest
    {
        private readonly PlaceOrderHandler _handler;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Product> _productRespository;
        private readonly IRepository<User> _userRepository;
        private readonly Mock<IEventProducer> _mockEventProducer;
        private readonly Mock<ILogger<PlaceOrderHandler>> _mockLogger;
    }
}