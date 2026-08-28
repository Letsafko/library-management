using Application.Features.Members.Create;

namespace Api.Endpoints.Members.Create;

internal static class Extensions
{
    internal static CreateMemberCommand? ToCommand(this Request? request)
    {
        return request is null
            ? null
            : new CreateMemberCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.MembershipType);
    }
}
