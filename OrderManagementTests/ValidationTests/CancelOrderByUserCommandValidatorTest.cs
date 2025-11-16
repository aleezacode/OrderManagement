using System;
using MongoDB.Bson;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Validators.Order;

namespace OrderManagementTests.ValidationTests
{
    [Trait("Category", "Validation")]
    public class CancelOrderByUserCommandValidatorTest
    {
        private readonly CancelOrderByUserCommandValidator _validator;

        public CancelOrderByUserCommandValidatorTest()
        {
            _validator = new CancelOrderByUserCommandValidator();
        }

        [Fact]
        public void ValidateCommandOK()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var reason = "Test Reason";

            var command = new CancelOrderByUserCommand(id, reason);

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCommandNotOK_IdIsNull()
        {
            var reason = "Test Reason";

            var command = new CancelOrderByUserCommand(null, reason);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ReasonIsNull()
        {
            var id = ObjectId.GenerateNewId().ToString();

            var command = new CancelOrderByUserCommand(id, null);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Reason is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ReasonIsEmpty()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var reason = "";

            var command = new CancelOrderByUserCommand(id, reason);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Reason is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_ReasonIsTooLong()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var reason = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzAB52";

            var command = new CancelOrderByUserCommand(id, reason);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Reason cannot be longer than 50 characters", result.Errors[0].ErrorMessage);
        }
    }
}