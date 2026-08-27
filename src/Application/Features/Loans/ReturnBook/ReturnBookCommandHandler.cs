using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books;
using Application.Messaging;
using Domain.Members;
using SharedKernel.Primitives;

namespace Application.Features.Loans.ReturnBook;

public sealed class  ReturnBookCommandHandler(
    IMemberRepository memberRepository,
    IBookRepository bookRepository,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<ReturnBookCommand, ReturnBookResponse>
{
    public async Task<Result<ReturnBookResponse>> HandleAsync(ReturnBookCommand? request, CancellationToken cancellationToken)
    {
        var member = await memberRepository.GetByIdAsync(request!.MemberId, cancellationToken);
        if (member is null)
        {
            return MemberErrors.MemberNotFound;
        }

        var activeLoan = member.GetActiveLoan(request.BookCopyId);
        if (activeLoan is null)
        {
            return MemberErrors.LoanNotFound;
        }

        var currentDatetime = dateTimeProvider.UtcNow;
        var penaltyResult = member.ReturnBook(request.BookCopyId, currentDatetime);
        if (!penaltyResult.IsSuccess)
        {
            return penaltyResult.Error;
        }

        var book = await bookRepository.GetBookByBookCopyIdAsync(request.BookCopyId, cancellationToken);
        var result = book!.MarkCopyAsReturned(request.BookCopyId);
        if(!result.IsSuccess)
        {
            return result.Error;
        }

        await memberRepository.UpdateAsync(member, cancellationToken);
        await bookRepository.UpdateAsync(book, cancellationToken);

        return new ReturnBookResponse(
            memberId: request.MemberId,
            bookCopyId: request.BookCopyId,
            returnedAt: currentDatetime,
            daysLate: activeLoan.GetDaysLate(currentDatetime),
            penalty: penaltyResult.Value);
    }
}
