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

namespace OrderManagement.Handlers.Order
{
    public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, string>
    {
        private readonly IRepository<OrderModel> _orderRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IEventProducer _eventProducer;

        public PlaceOrderHandler(IRepository<OrderModel> orderRepository, IRepository<Product> productRepository, IEventProducer eventProducer)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _eventProducer = eventProducer;
        }

        public async Task<string> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            var orderItems = await BuildeOrderItems(request.Items);
            var order = new OrderModel
            {
                UserId = request.UserId,
                Items = orderItems,
                OrderStatus = OrderStatus.Placed,
                TotalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity),
                CreatedAt = DateTime.UtcNow
            };
            var insertedOrder = await _orderRepository.CreateAsync(order);
            //Publish event

            var orderPlacedEvent = new OrderPlaced
            {
            OrderId = insertedOrder.Id,
            UserId = order.UserId,
            Items = order.Items.Select(i => new OrderItemEvent
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalAmount = order.TotalAmount
            };

            await _eventProducer.ProduceAsync("orders", orderPlacedEvent, cancellationToken);

            return insertedOrder.Id;
        }

        private async Task<List<OrderItemModel>> BuildeOrderItems(List<Commands.Order.OrderItem> items)
        {
            var orderItems = new List<OrderItemModel>();

            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new Exception($"Product with ID {item.ProductId} not found.");
                }

                orderItems.Add(new OrderItemModel
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            return orderItems;
        }
    }
}