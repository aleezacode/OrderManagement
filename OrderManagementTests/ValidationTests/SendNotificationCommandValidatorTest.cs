using System;
using MongoDB.Bson;
using OrderManagement.Commands.Notification;
using OrderManagement.Validators.Notification;

namespace OrderManagementTests.ValidationTests
{
    [Trait("Category", "Validation")]
    public class SendNotificationCommandValidatorTest
    {
        private readonly SendNotificationCommandValidator _validator;

        public SendNotificationCommandValidatorTest()
        {
            _validator = new SendNotificationCommandValidator();
        }

        [Fact]
        public void ValidateCommandOK()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var message = "Test message for validating";
            var command = new SendNotificationCommand(id, message);

            var result = _validator.Validate(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void ValidateCommandNotOK_IdIsNull()
        {
            var message = "Test message for validating";
            var command = new SendNotificationCommand(null, message);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_MessageIsNull()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var command = new SendNotificationCommand(id, null);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Message is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_MessageIsEmpty()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var message = "";
            var command = new SendNotificationCommand(id, message);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Message is required", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void ValidateCommandNotOK_MessageLengthIsTooLong()
        {
            var id = ObjectId.GenerateNewId().ToString();
            var message = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ABCD253";
            var command = new SendNotificationCommand(id, message);

            var result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Message cannot be longer than 250 characters", result.Errors[0].ErrorMessage);
        }
    }
}