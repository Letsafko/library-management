using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books.Abstracts;
using Application.Messaging;
using Domain.Books;
using SharedKernel.Primitives;

namespace Application.Features.Books.Create;

public sealed class CreateBookCommandHandler(
    IBookRepository bookRepository,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateBookCommand, BookResponse>
{
    public async Task<Result<BookResponse>> HandleAsync(CreateBookCommand? request, CancellationToken cancellationToken)
    {
        var book = Book.Create(
            request!.Isbn!,
            request.Title!,
            request.Author!,
            dateTimeProvider.UtcNow);

        if (!book.IsSuccess)
        {
            return book.Error;
        }
        
        await bookRepository.AddAsync(book.Value, cancellationToken);
        return new BookResponse(book.Value);
    }
}