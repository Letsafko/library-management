using Application.Features.Books.Create;

namespace Api.Endpoints.Books.Create;

internal static class Extensions
{
    internal static CreateBookCommand? ToCommand(this Request? request)
    {
        return request is null
            ? null
            : new CreateBookCommand(
                request.Title,
                request.Author,
                request.Isbn);
    }
}