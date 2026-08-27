using FluentValidation;
using SharedKernel.Primitives;

namespace Application;

internal static class RuleBuilderOptionsExtensions
{
    internal static IRuleBuilderOptions<T, TProperty> AddError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> options,
        ErrorResult error)
    {
        return options
            .WithMessage(error.Description)
            .WithErrorCode(error.Code);
    }
}