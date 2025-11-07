using System;
using MongoDB.Bson;
using OrderManagement.Commands.Order;
using OrderManagement.Validators.Order;

namespace OrderManagementTests.ValidationTests
{
    public class PlaceOrderCommandValidatorTest
    {
        private readonly PlaceOrderCommandValidator _validator;

        public PlaceOrderCommandValidatorTest()
        {
            _validator = new PlaceOrderCommandValidator();
        }

        [Fact]
        public void ValidateCommandOK()
        {
            var item = new OrderItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 2,
                UnitPrice = 29.99M

            };
            var id = ObjectId.GenerateNewId().ToString();

            var command = new PlaceOrderCommand(id, item);

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCommandNotOK_IdIsNull()
        {
            var item = new OrderItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 2,
                UnitPrice = 29.99M

            };

            var command = new PlaceOrderCommand(null, item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ItemIsEmpty()
        {
            var id = ObjectId.GenerateNewId().ToString();

            var command = new PlaceOrderCommand(id, null);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Item property is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ProductIdIsNull()
        {
            var item = new OrderItem
            {
                ProductId = null,
                Quantity = 2,
                UnitPrice = 29.99M

            };
            var id = ObjectId.GenerateNewId().ToString();

            var command = new PlaceOrderCommand(id, item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_QuantityIsLessThan1()
        {
            var item = new OrderItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 0,
                UnitPrice = 29.99M

            };
            var id = ObjectId.GenerateNewId().ToString();

            var command = new PlaceOrderCommand(id, item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Quantity must be greater than 0", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_QuantityIsGreaterThan5()
        {
            var item = new OrderItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 6,
                UnitPrice = 29.99M

            };
            var id = ObjectId.GenerateNewId().ToString();

            var command = new PlaceOrderCommand(id, item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Quantity must be less than or equal to 5", result.Errors[0].ErrorMessage);
        }
    }
}