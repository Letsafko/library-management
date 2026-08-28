using Application.Features.Models;

namespace Api.ViewModels;

public sealed record MemberViewModel
{
    public MemberViewModel() { }

    public MemberViewModel(MemberResponse response)
    {
        MemberId = response.MemberId;
        FirstName = response.FirstName;
        LastName = response.LastName;
        Email = response.Email;
        MembershipType = response.MembershipType.Name;
    }

    public int MemberId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MembershipType { get; init; } = string.Empty;
}
