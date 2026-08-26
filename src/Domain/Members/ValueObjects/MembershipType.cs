using System;
using System.Collections.Generic;
using SharedKernel.Primitives;

namespace Domain.Members.ValueObjects;

public sealed class MembershipType : ValueObject
{
    public static readonly MembershipType Standard = new("Standard", 3, TimeSpan.FromDays(21));
    public static readonly MembershipType Student = new("Student", 5, TimeSpan.FromDays(28));

    private MembershipType(string name, int maxSimultaneousLoans, TimeSpan loanDuration)
    {
        MaxSimultaneousLoans = maxSimultaneousLoans;
        LoanDuration = loanDuration;
        Name = name;
    }
    
    public int MaxSimultaneousLoans { get; }
    public TimeSpan LoanDuration { get; }
    public string Name { get; }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }
}
