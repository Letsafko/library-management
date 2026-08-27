using System;
using Domain.Loans.ValueObjects;

namespace Application.Features.Loans;

public sealed record ReturnBookResponse
{
    public ReturnBookResponse(int memberId, int bookCopyId, DateTime returnedAt, int daysLate, Money penalty)
    {
        MemberId = memberId;
        BookCopyId = bookCopyId;
        ReturnedAt = returnedAt;
        DaysLate = daysLate;
        PenaltyAmount = penalty.Value;
    }

    public int MemberId { get; }
    public int BookCopyId { get; }
    public DateTime ReturnedAt { get; }
    public int DaysLate { get; }
    public decimal PenaltyAmount { get; }
}
