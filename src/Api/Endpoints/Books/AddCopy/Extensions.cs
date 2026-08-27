using Application.Features.Books.AddCopy;

namespace Api.Endpoints.Books.AddCopy;

internal static class Extensions
{
    internal static AddBookCopyCommand? ToCommand(this Request? request, int bookId)
    {
        return request is null
            ? null
            : new AddBookCopyCommand(bookId, request.Isbn);
    }
}
