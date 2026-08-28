using Application.Features.Books.Borrow;

namespace Api.Endpoints.Books.Borrow;

internal static class Extensions
{
    internal static BorrowBookCommand? ToCommand(this Request? request, int memberId)
    {
        return request is null
            ? null
            : new BorrowBookCommand(memberId, request.BookCopyId);
    }
}
