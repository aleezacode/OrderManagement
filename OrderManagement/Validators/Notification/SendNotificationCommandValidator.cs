using System;
using FluentValidation;
using MongoDB.Bson;
using OrderManagement.Commands.Notification;

namespace OrderManagement.Validators.Notification
{
    public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
    {
        public SendNotificationCommandValidator()
        {
            RuleFor(x => x.OrderId).MustBeValidObjectId();

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required")
                .MaximumLength(250).WithMessage("Message cannot be longer than 250 characters");
        }
    }
}