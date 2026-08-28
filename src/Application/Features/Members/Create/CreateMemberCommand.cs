using Application.Messaging;

namespace Application.Features.Members.Create;

public sealed record CreateMemberCommand(
    string? FirstName,
    string? LastName,
    string? Email,
    string? MembershipType) : ICommand;
