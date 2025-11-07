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
                .NotNull().WithMessage("{PropertyName} is required");
        }
    }
}