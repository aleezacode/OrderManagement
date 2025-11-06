using System;
using FluentValidation;
using MongoDB.Bson;

namespace OrderManagement.Validators
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> MustBeValidObjectId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("{PropertyName} is required")
                .Must(id => !string.IsNullOrEmpty(id) && ObjectId.TryParse(id, out _))
                .WithMessage("{PropertyName} must be a valid ObjectId");
        }
    }
}