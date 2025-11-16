using System;
using MongoDB.Bson;
using OrderManagement.Commands.Inventory;
using OrderManagement.Validators.Inventory;

namespace OrderManagementTests.ValidationTests
{
    [Trait("Category", "Validation")]
    public class ReleaseStockCommandValidatorTest
    {
        private readonly ReleaseStockCommandValidator _validator;

        public ReleaseStockCommandValidatorTest()
        {
            _validator = new ReleaseStockCommandValidator();
        }

        [Fact]
        public void ValidateCommandOK()
        {
            var command = new ReleaseStockCommand(ObjectId.GenerateNewId().ToString(), 2);

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCommandNotOK_IdIsNull()
        {
            var command = new ReleaseStockCommand(null, 2);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_QuantityIsLessThan1()
        {
            var command = new ReleaseStockCommand(ObjectId.GenerateNewId().ToString(), 0);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Quantity requested to release has to be greater than 0", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_QuantityGreaterThan5()
        {
            var command = new ReleaseStockCommand(ObjectId.GenerateNewId().ToString(), 6);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Cannot release more than 5 units at once", result.Errors[0].ErrorMessage);
        }
    }
}