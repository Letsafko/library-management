using System.Collections.Generic;

namespace SharedKernel.Primitives;

public sealed record ValidationError : ErrorResult
{
    public ValidationError(params ErrorResult[] errors) : base("Validation.General", "One or more validation errors occurred.", ErrorType.Validation)
    {
        Errors = errors;
    }

    public IReadOnlyList<ErrorResult> Errors { get; }
}