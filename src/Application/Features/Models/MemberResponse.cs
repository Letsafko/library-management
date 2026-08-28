using Domain.Members;
using Domain.Members.ValueObjects;

namespace Application.Features.Models;

public sealed record MemberResponse
{
    public MemberResponse(Member member)
    {
        MemberId = member.Id;
        FirstName = member.FirstName;
        LastName = member.LastName;
        Email = member.Email;
        MembershipType = member.MembershipType;
    }

    public int MemberId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public MembershipType MembershipType { get; }
}
