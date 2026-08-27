using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Books;
using Domain.Loans;
using Domain.Loans.ValueObjects;
using Domain.Members.ValueObjects;
using SharedKernel.Primitives;

namespace Domain.Members;

public class Member : Entity<int>
{
    private readonly List<Loan> _loans;
    protected Member(
        string firstName,
        string lastName,
        string email,
        MembershipType membershipType,
        DateTime createdDatetime,
        DateTime lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        MembershipType = membershipType;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        _loans = [];
    }
    
    public static Member Create(
        string firstName,
        string lastName,
        string email,
        MembershipType membershipType,
        DateTime createdDatetime)
    {
        return new Member(
            firstName,
            lastName, 
            email,
            membershipType,
            createdDatetime,
            createdDatetime);
    }
    
    public bool HasReachedLoanLimit => ActiveLoansCount >= MembershipType.MaxSimultaneousLoans;
    public int ActiveLoansCount => _loans.Count(l => !l.IsReturned);
    public MembershipType MembershipType { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email {get; private set; }
    public IReadOnlyList<Loan> Loans => _loans;
    
    public Result<Loan> BorrowBook(BookCopy bookCopy, DateTime currentDatetime)
    {
        if (HasReachedLoanLimit)
        {
            return MemberErrors.LoanLimitReached;
        }

        var markBorrowedResult = bookCopy.MarkAsBorrowed();
        if (!markBorrowedResult.IsSuccess)
        {
            return markBorrowedResult.Error;
        }

        var loan = Loan.Create(Id, bookCopy.Id, MembershipType.LoanDuration, currentDatetime);
        _loans.Add(loan);
        
        return loan;
    }
    
    public Loan? GetActiveLoan(int bookCopyId)
    {
        return _loans.FirstOrDefault(l => l.BookCopyId == bookCopyId && !l.IsReturned);
    }
    
    public Result<Money> ReturnBook(int bookCopyId, DateTime currentDatetime)
    {
        var loan = GetActiveLoan(bookCopyId);
        if (loan == null)
        {
            return MemberErrors.LoanNotFound;
        }
        
        var returnResult = loan.Return(currentDatetime);
        if (!returnResult.IsSuccess)
        {
            return returnResult.Error;
        }

        var daysLate = loan.GetDaysLate(currentDatetime);
        var penalty = CalculatePenalty(daysLate);
        return penalty;
    }
    
    private static Result<Money> CalculatePenalty(int daysLate)
    {
        if (daysLate <= 0)
        {
            return Money.Zero;
        }

        const decimal maxPenaltyAmount = 10.00m;
        const decimal penaltyPerDay = 0.20m;
        
        var totalPenalty = daysLate * penaltyPerDay;
        var cappedPenalty = Math.Min(totalPenalty, maxPenaltyAmount);

        return Money.Create(cappedPenalty);
    }
}