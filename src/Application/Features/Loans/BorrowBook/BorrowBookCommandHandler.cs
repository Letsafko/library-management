using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books;
using Application.Messaging;
using Domain.Books;
using Domain.Members;
using SharedKernel.Primitives;

namespace Application.Features.Loans.BorrowBook;

public sealed class BorrowBookCommandHandler(
    IMemberRepository memberRepository,
    IBookRepository bookRepository,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<BorrowBookCommand, LoanResponse>
{
    public async Task<Result<LoanResponse>> HandleAsync(BorrowBookCommand? request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request!.MemberId, cancellationToken);
        if (member is null)
        {
            return MemberErrors.MemberNotFound;
        }

        var bookCopy = await bookRepository.GetBookCopyByIdAsync(request.BookCopyId, cancellationToken);
        if (bookCopy is null)
        {
            return BookErrors.BookCopyNotFound;
        }

        var result = member.BorrowBook(bookCopy, dateTimeProvider.UtcNow);
        if (!result.IsSuccess)
        {
            return result.Error;
        }

        await memberRepository.UpdateAsync(member, bookCopy, cancellationToken);
        return new LoanResponse(result.Value);
    }
}
