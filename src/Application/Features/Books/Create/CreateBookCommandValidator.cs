using Domain.Books;
using FluentValidation;

namespace Application.Features.Books.Create;

public sealed class CreateBookCommandValidator : AbstractValidator<CreateBookCommand> 
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Author)
            .NotEmpty()
            .AddError(BookErrors.MissingBookAuthor);
        
        RuleFor(x => x.Title)
            .NotEmpty()
            .AddError(BookErrors.MissingBookTitle);
        
        RuleFor(x => x.Isbn)
            .NotEmpty()
            .AddError(BookErrors.MissingBookIsbn);
    }
}