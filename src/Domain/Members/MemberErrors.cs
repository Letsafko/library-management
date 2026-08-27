using SharedKernel.Primitives;

namespace Domain.Members;

public static class MemberErrors
{
    public static ErrorResult LoanLimitReached => ErrorResult.Problem(
        code: "Member.LoanLimitReached",
        description: "Load limit exceeded.");
        
    public static ErrorResult LoanNotFound => ErrorResult.Problem(
        code: "Member.LoanNotFound",
        description: "Member loan not found.");

    public static ErrorResult MemberNotFound => ErrorResult.NotFound(
        code: "Member.NotFound",
        description: "Member not found.");

    public static ErrorResult InvalidMemberId => ErrorResult.Problem(
        code: "Member.InvalidId",
        description: "Member id must be greater than zero.");
}