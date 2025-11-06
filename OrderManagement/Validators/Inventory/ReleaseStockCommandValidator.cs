using System;
using FluentValidation;
using MongoDB.Bson;
using OrderManagement.Commands.Inventory;

namespace OrderManagement.Validators.Inventory
{
    public class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
    {
        public ReleaseStockCommandValidator()
        {
            RuleFor(x => x.ProductId).MustBeValidObjectId();

            RuleFor(x => x.ReleaseQuantity)
                .GreaterThan(0).WithMessage("Quantity requested to release has to be greater than 0")
                .LessThanOrEqualTo(5).WithMessage("Cannot release more than 5 units at once");
        }
    }
}