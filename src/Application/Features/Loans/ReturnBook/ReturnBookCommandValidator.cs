using Domain.Books;
using Domain.Members;
using FluentValidation;

namespace Application.Features.Loans.ReturnBook;

public sealed class ReturnBookCommandValidator : AbstractValidator<ReturnBookCommand>
{
    public ReturnBookCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .AddError(MemberErrors.InvalidMemberId);

        RuleFor(x => x.BookCopyId)
            .GreaterThan(0)
            .AddError(BookErrors.InvalidBookCopyId);
    }
}
