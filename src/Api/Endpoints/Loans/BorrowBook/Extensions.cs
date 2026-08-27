using Application.Features.Loans.BorrowBook;

namespace Api.Endpoints.Loans.BorrowBook;

internal static class Extensions
{
    internal static BorrowBookCommand? ToCommand(this Request? request, int memberId)
    {
        return request is null
            ? null
            : new BorrowBookCommand(memberId, request.BookCopyId);
    }
}
