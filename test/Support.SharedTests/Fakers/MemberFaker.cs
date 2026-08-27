using System;
using Bogus;
using Domain.Members.ValueObjects;
using Support.SharedTests.Stubs;

namespace Support.SharedTests.Fakers;

public sealed class MemberFaker : Faker<MemberStub>
{
    public MemberFaker(
        int id = 0,
        MembershipType? membershipType = null,
        DateTime? createdDatetime = null,
        DateTime? lastModifiedDatetime = null)
    {
        CustomInstantiator(f =>
        {
            var firstName = f.Name.FirstName();
            var lastName = f.Name.LastName();
            var email = f.Internet.Email(firstName, lastName);
            var createdDate = createdDatetime ?? f.Date.Past(2);
            var lastModifiedDate = lastModifiedDatetime ?? f.Date.Between(createdDate, DateTime.UtcNow);

            return new MemberStub(
                id,
                firstName,
                lastName,
                email,
                membershipType ?? f.PickRandom(MembershipType.Standard, MembershipType.Student),
                createdDate,
                lastModifiedDate);
            
        });
    }
}