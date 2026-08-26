using SharedKernel.Primitives;

namespace Domain.Loans;

internal static class LoanErrors
{
    internal static ErrorResult AlreadyReturned => ErrorResult.Problem(
        code: "Loan.AlreadyReturned", 
        description: "Loan already returned.");
}