using SharedKernel.Primitives;

namespace Domain.Members;

internal static class MemberErrors
{
    internal static ErrorResult LoanLimitReached => ErrorResult.Problem(
        code: "Member.LoanLimitReached",
        description: "Load limit exceeded.");
        
    internal static ErrorResult BookNotAvailable => ErrorResult.Problem(
        code: "Member.BookNotAvailable",
        description: "Book not available.");
    
    internal static ErrorResult LoanNotFound => ErrorResult.Problem(
        code: "Member.LoanNotFound",
        description: "Member loan not found.");
}