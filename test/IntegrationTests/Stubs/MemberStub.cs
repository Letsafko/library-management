using System;
using Bogus;
using Domain.Members;
using Domain.Members.ValueObjects;

namespace IntegrationTests.Stubs;

internal sealed class MemberStubFaker : Faker<MemberStubFaker.MemberStub>
{
    public MemberStubFaker(
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
                firstName,
                lastName,
                email,
                membershipType ?? f.PickRandom<MembershipType>(MembershipType.Standard, MembershipType.Student),
                createdDate,
                lastModifiedDate);
            
        });
    }
    
    internal sealed class MemberStub(
        string firstName,
        string lastName,
        string email,
        MembershipType membershipType,
        DateTime createdDate,
        DateTime lastModifiedDate) : Member(firstName, lastName, email, membershipType, createdDate, lastModifiedDate);
}