namespace Api.Endpoints.Books.Create;

public sealed class Request
{
    public string? Author { get; init; }
    public string? Title { get; init; }
    public string? Isbn { get; init; }
}