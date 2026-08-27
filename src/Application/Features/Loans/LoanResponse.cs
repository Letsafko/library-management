using System;
using Domain.Loans;

namespace Application.Features.Loans;

public sealed record LoanResponse
{
    public LoanResponse(Loan loan)
    {
        LoanId = loan.Id;
        MemberId = loan.MemberId;
        BookCopyId = loan.BookCopyId;
        BorrowedAt = loan.BorrowedAt;
        DueDate = loan.DueDate;
    }

    public int LoanId { get; }
    public int MemberId { get; }
    public int BookCopyId { get; }
    public DateTime BorrowedAt { get; }
    public DateTime DueDate { get; }
}
