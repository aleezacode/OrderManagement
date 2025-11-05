using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OrderManagement.Commands.Order;
using OrderManagement.Repositories;
using OrderModel = OrderManagement.Models.Order;
using OrderItemModel = OrderManagement.Models.OrderItem;
using OrderManagement.Models;
using OrderManagement.Kafka;
using OrderManagement.Models.Events.Orders;
using OrderItemEvent = OrderManagement.Models.Events.Orders.OrderItem;
using MongoDB.Bson;

namespace OrderManagement.Handlers.Order
{
    public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, string>
    {
        private readonly IRepository<OrderModel> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IEventProducer _eventProducer;
        private readonly ILogger<PlaceOrderHandler> _logger;

        public PlaceOrderHandler(IRepository<OrderModel> orderRepository, IRepository<Product> productRepository, IEventProducer eventProducer, ILogger<PlaceOrderHandler> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _eventProducer = eventProducer;
            _logger = logger;
        }

        public async Task<string> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling PlaceOrder command for user: {request.UserId}");
            
                var orderItem = await BuildeOrderItems(request.Item);
                var order = new OrderModel
                {
                    UserId = request.UserId,
                    Item = orderItem,
                    OrderStatus = OrderStatus.Placed,
                    TotalAmount = orderItem.UnitPrice * orderItem.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                var insertedOrder = await _orderRepository.CreateAsync(order);

                _logger.LogInformation($"Order successfully inserted for user: {request.UserId} with orderId {order.Id}");

                var orderPlacedEvent = new OrderPlaced
                {
                    OrderId = insertedOrder.Id,
                    UserId = order.UserId,
                    Item = new OrderItemEvent
                    {
                        ProductId = order.Item.ProductId,
                        Quantity = order.Item.Quantity,
                        UnitPrice = order.Item.UnitPrice
                    },
                    TotalAmount = order.TotalAmount
                };

                _logger.LogInformation($"Publishing OrderPlaced event for order: {order.Id}");

                await _eventProducer.ProduceAsync("order-placed", orderPlacedEvent, cancellationToken);

                _logger.LogInformation($"Successfully published OrderPlaced event for order: {order.Id}");

                return insertedOrder.Id!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to place order for user: {request.UserId}");
                throw;
            }
        }

        private async Task<OrderItemModel> BuildeOrderItems(Commands.Order.OrderItem item)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    _logger.LogError($"Product not found for id: {item.ProductId}");
                    throw new Exception($"Product with ID {item.ProductId} not found.");
                }

                return new OrderItemModel()
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error building orderItem for product: {item.ProductId}");
                throw;
            }
        }
    }
}