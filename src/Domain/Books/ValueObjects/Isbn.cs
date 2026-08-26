using System;
using System.Collections.Generic;
using SharedKernel.Primitives;

namespace Domain.Books.ValueObjects;

public sealed class Isbn : ValueObject, IValueObject<Isbn, string>
{
    private const int ThirteenDigits = 13;
    private const int TenDigits = 10;
    private Isbn(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Isbn> Create(string value)
    {
        var normalized = value
            .Replace("-", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" ", "", StringComparison.OrdinalIgnoreCase);

        if (normalized.Length != TenDigits && normalized.Length != ThirteenDigits)
        {
            return BookErrors.InvalidIsbnLength;
        }

        return new Isbn(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}