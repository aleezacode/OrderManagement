using System;
using MediatR;
using OrderManagement.Commands.Payment;
using OrderManagement.Exceptions;
using OrderManagement.Kafka;
using OrderManagement.Models.Enums;
using OrderManagement.Models.Events.Payments;
using OrderManagement.Repositories;
using OrderModel = OrderManagement.Models.Order;
using PaymentModel = OrderManagement.Models.Payment;

namespace OrderManagement.Handlers.Payment;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, bool>
{
    private readonly IRepository<OrderModel> _orderRepository;
    private readonly IRepository<PaymentModel> _paymentRepository;
    private readonly IEventProducer _eventProducer;
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(IRepository<OrderModel> orderRepository, IRepository<PaymentModel> paymentRepository, IEventProducer eventProducer, ILogger<ProcessPaymentHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _eventProducer = eventProducer;
        _logger = logger;
    }
    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Handling ProcessPayment command for order: {request.orderId}");
            var order = await _orderRepository.GetByIdAsync(request.orderId);

            if (order.OrderStatus != OrderStatus.Placed)
            {
                _logger.LogWarning($"Unable to process payment. Order status is {order.OrderStatus}");
                return false;
            }

            var payment = new PaymentModel
            {
              OrderId = request.orderId,
              UserId = order.UserId,
              Amount = order.TotalAmount,
              PaymentStatus = PaymentStatus.Processed,
              ProcessedAt = DateTime.UtcNow
            };

            await _paymentRepository.CreateAsync(payment);
            _logger.LogInformation($"Payment record sucessfully created for orderId {request.orderId} with id {payment.Id}");

            order.OrderStatus = OrderStatus.Paid;
            await _orderRepository.UpdateAsync(order.Id, order);
            _logger.LogInformation($"Order status updated to paid for orderId {request.orderId}");

            var paymentProcessedEvent = new PaymentProcessed
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                UserId = payment.UserId,
                Amount = payment.Amount
            };

            _logger.LogInformation($"Publishing PaymentProcessed event for orderId {request.orderId}");
            await _eventProducer.ProduceAsync("payment-processed", paymentProcessedEvent, cancellationToken);
            _logger.LogInformation($"Sucessfully published PaymentProcessed event for orderId {request.orderId}");
            
            return true;
        }
        catch (DocumentNotFoundException ex)
        {
            _logger.LogError(ex, $"Order with id {request.orderId} not found");
            throw;
        }
        catch (DocumentCreationFailedException ex)
        {
            _logger.LogError(ex, $"Failed to create payment record for orderId {request.orderId}");
            throw;
        }
        catch (DocumentUpdatedFailedException ex)
        {
            _logger.LogError(ex, $"Failed to update status for order with id {request.orderId}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error while processing payment for orderId {request.orderId}");
            throw;
        }
    }
}
