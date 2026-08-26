using System;
using System.Collections.Generic;
using System.Linq;
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

    private static readonly IReadOnlyList<MembershipType> All = new List<MembershipType> { Standard, Student };
    public static MembershipType GetByName(string membershipName)
    {
        return All.Single(mt => mt.Name == membershipName);
    }
    
    public int MaxSimultaneousLoans { get; }
    public TimeSpan LoanDuration { get; }
    public string Name { get; }
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }
    
}
