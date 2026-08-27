using System;
using Application.Features.Loans;

namespace Api.ViewModels;

public sealed record ReturnBookViewModel
{
    public ReturnBookViewModel() { }

    public ReturnBookViewModel(ReturnBookResponse response)
    {
        MemberId = response.MemberId;
        BookCopyId = response.BookCopyId;
        ReturnedAt = response.ReturnedAt;
        DaysLate = response.DaysLate;
        PenaltyAmount = response.PenaltyAmount;
    }

    public int MemberId { get; init; }
    public int BookCopyId { get; init; }
    public DateTime ReturnedAt { get; init; }
    public int DaysLate { get; init; }
    public decimal PenaltyAmount { get; init; }
}
