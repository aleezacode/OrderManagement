using System;
using FluentValidation;
using MongoDB.Bson;
using OrderManagement.Commands.Order;

namespace OrderManagement.Validators.Order
{
    public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(x => x.OrderId).MustBeValidObjectId();

            RuleFor(x => x.OrderStatus)
                .NotEmpty().WithMessage("OrderStatus is required")
                .IsInEnum().WithMessage("OrderStatus must be a valid enum");
        }
    }
}