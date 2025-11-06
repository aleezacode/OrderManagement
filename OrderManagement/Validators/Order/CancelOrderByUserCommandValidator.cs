using System;
using FluentValidation;
using MongoDB.Bson;
using OrderManagement.Commands.Order.Cancellation;

namespace OrderManagement.Validators.Order
{
    public class CancelOrderByUserCommandValidator : AbstractValidator<CancelOrderByUserCommand>
    {
        public CancelOrderByUserCommandValidator()
        {
            RuleFor(x => x.OrderId).MustBeValidObjectId();

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MaximumLength(50).WithMessage("Reason cannot be longer than 50 characters");
        }
    }
}