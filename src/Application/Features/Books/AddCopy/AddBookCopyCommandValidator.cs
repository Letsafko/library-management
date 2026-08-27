using Domain.Books;
using FluentValidation;

namespace Application.Features.Books.AddCopy;

public sealed class AddBookCopyCommandValidator : AbstractValidator<AddBookCopyCommand>
{
    public AddBookCopyCommandValidator()
    {
        RuleFor(x => x.BookId)
            .GreaterThan(0)
            .AddError(BookErrors.InvalidBookId);

        RuleFor(x => x.Isbn)
            .NotEmpty()
            .AddError(BookErrors.MissingBookIsbn);
    }
}
