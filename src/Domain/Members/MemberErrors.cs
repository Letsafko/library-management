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

    public static ErrorResult MissingFirstName => ErrorResult.Problem(
        code: "Member.MissingFirstName",
        description: "First name is required.");

    public static ErrorResult MissingLastName => ErrorResult.Problem(
        code: "Member.MissingLastName",
        description: "Last name is required.");

    public static ErrorResult MissingEmail => ErrorResult.Problem(
        code: "Member.MissingEmail",
        description: "Email is required.");

    public static ErrorResult MissingMembershipType => ErrorResult.Problem(
        code: "Member.MissingMembershipType",
        description: "Membership type is required.");

    public static ErrorResult InvalidMembershipType => ErrorResult.Problem(
        code: "Member.InvalidMembershipType",
        description: "Membership type is invalid. Valid values are: Standard, Student.");
}