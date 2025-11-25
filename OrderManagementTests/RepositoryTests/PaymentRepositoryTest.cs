using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using MongoDB.Bson;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Enums;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    [Trait("Category", "Repository")]
    public class PaymentRepositoryTest : BaseRepositoryTest
    {
        private PaymentRepository _paymentRepository;
        public PaymentRepositoryTest() : base("Payments")
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _paymentRepository = new PaymentRepository(Database, MongoDbSettings);
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
        public async Task CreatePaymentNotOk_ThrowException()
        {
            var exception = await Assert.ThrowsAsync<DocumentCreationFailedException>(
                async () => await _paymentRepository.CreateAsync(null)
            );

            Assert.Equal("Payment", exception.DocumentType);
            Assert.Contains("Failed to create", exception.Message);
        }

        [Fact]
        public async Task GetPaymentByIdOK()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 29.99m,
                PaymentStatus = PaymentStatus.Processed,
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
        public async Task GetPaymentByIdNotOK_ThrowsException()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _paymentRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("Payment", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetPaymentByIdNotOk_IdIsNull_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _paymentRepository.GetByIdAsync(null)
            );

            Assert.Equal("Payment", exception.DocumentType);
            Assert.Null(exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task UpdatePaymentStatusOK()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 49.99m,
                PaymentStatus = PaymentStatus.Processed,
                UserId = "64a7f0c2e1b8c8f5d6a4e9b3"
            };

            var createdPayment = await _paymentRepository.CreateAsync(payment);
            createdPayment.PaymentStatus = PaymentStatus.Failed;
            createdPayment.ProcessedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(createdPayment.Id, createdPayment);

            var updatedPayment = await _paymentRepository.GetByIdAsync(createdPayment.Id!);

            Assert.NotNull(updatedPayment);
            Assert.Equal(PaymentStatus.Failed, updatedPayment.PaymentStatus);
            Assert.NotNull(updatedPayment.ProcessedAt);
        }

        [Fact]
        public async Task UpdatePaymentNotOk_ThrowsException()
        {
            var payment = new Payment
            {
                OrderId = ObjectId.GenerateNewId().ToString(),
                Amount = 29.99m,
                PaymentStatus = PaymentStatus.Processed,
                UserId = "64a7f0c2e1b8c8f5d6a4e9b2"
            };

            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentUpdatedFailedException>(
                async () => await _paymentRepository.UpdateAsync(nonExistentId, payment)
            );

            Assert.Equal("Payment", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("Failed to update", exception.Message);
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

            var exception = await Record.ExceptionAsync(async() =>
            {
                await _paymentRepository.GetByIdAsync(createdPayment.Id);
            });
            Assert.NotNull(exception);
            Assert.Equal(typeof(DocumentNotFoundException), exception.GetType());
        }
    }
}