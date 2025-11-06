using System;
using FluentValidation;
using MongoDB.Bson;
using OrderManagement.Commands.Order;

namespace OrderManagement.Validators.Order
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.UserId).MustBeValidObjectId();

            RuleFor(x => x.Item)
                .NotNull().WithMessage("Item property is required")
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ProductId).MustBeValidObjectId();

                    item.RuleFor(x => x.Quantity)
                        .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                        .LessThanOrEqualTo(5).WithMessage("Quantity must be less than or equal to 5");
                });
        }
    }
}