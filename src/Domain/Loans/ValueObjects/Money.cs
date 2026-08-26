using System.Collections.Generic;
using SharedKernel.Primitives;

namespace Domain.Loans.ValueObjects;

public sealed class Money : ValueObject, IValueObject<Money, decimal>
{
    public static readonly Money Zero = new(0);
    
    private static ErrorResult NegativeMoney => ErrorResult.Problem(
        code: "Money.Negative",
        description: "Amount should not be negative.");
    
    private Money(decimal amount)
    {
        Amount = amount;
    }

    private decimal Amount { get; }


    public decimal Value => Amount;
    
    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
        {
            return NegativeMoney;
        }

        return new Money(amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }
}