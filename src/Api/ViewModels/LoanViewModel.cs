using System;
using Application.Features.Loans;

namespace Api.ViewModels;

public sealed record LoanViewModel
{
    public LoanViewModel() { }

    public LoanViewModel(LoanResponse loanResponse)
    {
        LoanId = loanResponse.LoanId;
        MemberId = loanResponse.MemberId;
        BookCopyId = loanResponse.BookCopyId;
        BorrowedAt = loanResponse.BorrowedAt;
        DueDate = loanResponse.DueDate;
    }

    public int LoanId { get; init; }
    public int MemberId { get; init; }
    public int BookCopyId { get; init; }
    public DateTime BorrowedAt { get; init; }
    public DateTime DueDate { get; init; }
}
