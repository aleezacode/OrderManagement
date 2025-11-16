using System;
using MongoDB.Bson;
using OrderManagement.Commands.Inventory;
using OrderManagement.Validators.Inventory;

namespace OrderManagementTests.ValidationTests
{
    [Trait("Category", "Validation")]
    public class ReserveStockCommandValidatorTest
    {
        private readonly ReserveStockCommandValidator _validator;

        public ReserveStockCommandValidatorTest()
        {
            _validator = new ReserveStockCommandValidator();
        }

        [Fact]
        public void ValidateCommandOK()
        {
            var item = new ReservedItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 2
            };

            var command = new ReserveStockCommand(ObjectId.GenerateNewId().ToString(), item);

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCommandNotOK_IdIsNull()
        {
            var item = new ReservedItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 2
            };

            var command = new ReserveStockCommand(null, item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ItemIsEmpty()
        {
            var command = new ReserveStockCommand(ObjectId.GenerateNewId().ToString(), null);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("ReservedItem property is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ProductIdIsEmpty()
        {
            var item = new ReservedItem
            {
                ProductId = null,
                Quantity = 2
            };

            var command = new ReserveStockCommand(ObjectId.GenerateNewId().ToString(), item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_QuantityIsLessThan1()
        {
            var item = new ReservedItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 0
            };

            var command = new ReserveStockCommand(ObjectId.GenerateNewId().ToString(), item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Quantity must be greater than 0", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_QuantityIsGreaterThan5()
        {
            var item = new ReservedItem
            {
                ProductId = ObjectId.GenerateNewId().ToString(),
                Quantity = 6
            };

            var command = new ReserveStockCommand(ObjectId.GenerateNewId().ToString(), item);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Quantity must be less than or equal to 5", result.Errors[0].ErrorMessage);
        }
    }
}