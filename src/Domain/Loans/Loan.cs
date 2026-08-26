using System;
using SharedKernel.Primitives;

namespace Domain.Loans;

public sealed class Loan : Entity<int>
{
    private Loan(
        int memberId,
        int bookCopyId,
        DateTime borrowedAt,
        DateTime dueDate,
        DateTime? returnedAt,
        DateTime createdDatetime,
        DateTime lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        MemberId = memberId;
        BookCopyId = bookCopyId;
        BorrowedAt = borrowedAt;
        ReturnedAt = returnedAt;
        DueDate = dueDate;
    }

    public DateTime DueDate { get; }
    public int MemberId { get; private set; }
    public int BookCopyId { get; private set; }
    public DateTime BorrowedAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public bool IsReturned => ReturnedAt.HasValue;
    
    internal static Loan Create(
        int memberId, 
        int bookCopyId,
        TimeSpan loanDuration,
        DateTime currentDatetime)
    {
        var dueDate = currentDatetime.Add(loanDuration);

        return new Loan(
            memberId,
            bookCopyId,
            borrowedAt: currentDatetime,
            dueDate,
            returnedAt: null,
            currentDatetime,
            currentDatetime);
    }

    private bool IsOverdue(DateTime currentDate)
    {
        return !IsReturned && currentDate > DueDate;
    }

    public int GetDaysLate(DateTime currentDate)
    {
        if (!IsReturned && IsOverdue(currentDate))
        {
            return (currentDate.Date - DueDate.Date).Days;
        }

        if (IsReturned && ReturnedAt > DueDate)
        {
            return (ReturnedAt.Value.Date - DueDate.Date).Days;
        }

        return 0;
    }

    internal Result Return(DateTime returnedDatetime)
    {
        if (IsReturned)
        {
            return LoanErrors.AlreadyReturned;
        }

        ReturnedAt = returnedDatetime;
        return Result.Success();
    }
}
