using System;
using Domain.Members;
using Domain.Members.ValueObjects;

namespace Support.SharedTests.Stubs;

public sealed class MemberStub(
    int id,
    string firstName,
    string lastName,
    string email,
    MembershipType membershipType,
    DateTime createdDate,
    DateTime lastModifiedDate) : Member(firstName, lastName, email, membershipType, createdDate, lastModifiedDate)
{
    public new int Id { get; } = id;
};