namespace Api.Endpoints.Members.Create;

public sealed class Request
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? MembershipType { get; init; }
}
