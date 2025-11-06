using System;
using FluentValidation;
using MongoDB.Bson;
using OrderManagement.Commands.Inventory;

namespace OrderManagement.Validators.Inventory
{
    public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
    {
        public ReserveStockCommandValidator()
        {
            RuleFor(x => x.OrderId).MustBeValidObjectId();

            RuleFor(x => x.ReservedItem)
                .NotNull().WithMessage("ReservedItem property is required")
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