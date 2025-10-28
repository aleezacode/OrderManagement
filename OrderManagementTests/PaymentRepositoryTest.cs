using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using MongoDB.Bson;
using OrderManagement.Models;
using OrderManagement.Models.Enums;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    public class PaymentRepositoryTest : BaseIntegrationTest
    {
        private readonly PaymentRepository _paymentRepository;
        public PaymentRepositoryTest() : base("Payments")
        {
            _paymentRepository = new PaymentRepository(_mongoDBSettings);
        }

        [Fact]
        public async Task CreatePaymentOK()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 59.98m,
                PaymentStatus = PaymentStatus.Processed,
                UserId = "64a7f0c2e1b8c8f5d6a4e9b1"
            };

            var createdPayment = await _paymentRepository.CreateAsync(payment);
            Assert.NotNull(createdPayment);
            Assert.Equal(payment.OrderId, createdPayment.OrderId);
            Assert.Equal(payment.Amount, createdPayment.Amount);
            Assert.Equal(payment.PaymentStatus, createdPayment.PaymentStatus);
            Assert.Equal(payment.UserId, createdPayment.UserId);
        }

        [Fact]
        public async Task GetPaymentByIdOK()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 29.99m,
                PaymentStatus = PaymentStatus.Pending,
                UserId = "64a7f0c2e1b8c8f5d6a4e9b2"
            };

            var createdPayment = await _paymentRepository.CreateAsync(payment);
            var fetchedPayment = await _paymentRepository.GetByIdAsync(createdPayment.Id!);

            Assert.NotNull(fetchedPayment);
            Assert.Equal(createdPayment.Id, fetchedPayment.Id);
            Assert.Equal(createdPayment.OrderId, fetchedPayment.OrderId);
            Assert.Equal(createdPayment.Amount, fetchedPayment.Amount);
            Assert.Equal(createdPayment.PaymentStatus, fetchedPayment.PaymentStatus);
            Assert.Equal(createdPayment.UserId, fetchedPayment.UserId);
        }

        [Fact]
        public async Task UpdatePaymentStatusOK()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 49.99m,
                PaymentStatus = PaymentStatus.Pending,
                UserId = "64a7f0c2e1b8c8f5d6a4e9b3"
            };

            var createdPayment = await _paymentRepository.CreateAsync(payment);
            createdPayment.PaymentStatus = PaymentStatus.Processed;
            createdPayment.ProcessedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(createdPayment.Id, createdPayment);

            var updatedPayment = await _paymentRepository.GetByIdAsync(createdPayment.Id!);

            Assert.NotNull(updatedPayment);
            Assert.Equal(PaymentStatus.Processed, updatedPayment.PaymentStatus);
            Assert.NotNull(updatedPayment.ProcessedAt);
        }

        [Fact]
        public async Task DeletePaymentOK()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 19.99m,
                PaymentStatus = PaymentStatus.Failed,
                UserId = "64a7f0c2e1b8c8f5d6a4e9b4"
            };

            var createdPayment = await _paymentRepository.CreateAsync(payment);
            await _paymentRepository.DeleteAsync(createdPayment.Id!);

            var deletedPayment = await _paymentRepository.GetByIdAsync(createdPayment.Id!);
            Assert.Null(deletedPayment);
        }
    }
}