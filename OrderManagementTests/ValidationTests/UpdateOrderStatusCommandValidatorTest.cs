using System;
using MongoDB.Bson;
using OrderManagement.Commands.Order;
using OrderManagement.Validators.Order;

namespace OrderManagementTests.ValidationTests
{
    public class UpdateOrderStatusCommandValidatorTest
    {
        private readonly UpdateOrderStatusCommandValidator _validator;

        public UpdateOrderStatusCommandValidatorTest()
        {
            _validator = new UpdateOrderStatusCommandValidator();
        }

        [Fact]
        public void ValidateCommandOK()
        {
            var command = new UpdateOrderStatusCommand(ObjectId.GenerateNewId().ToString(), OrderStatus.Failed);

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCommandNotOK_IdIsNull()
        {
            var command = new UpdateOrderStatusCommand(null, OrderStatus.Failed);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }
    }
}