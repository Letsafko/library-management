using Domain.Books;
using Domain.Members;
using FluentValidation;

namespace Application.Features.Books.Borrow;

public sealed class BorrowBookCommandValidator : AbstractValidator<BorrowBookCommand>
{
    public BorrowBookCommandValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .AddError(MemberErrors.InvalidMemberId);

        RuleFor(x => x.BookCopyId)
            .GreaterThan(0)
            .AddError(BookErrors.InvalidBookCopyId);
    }
}
