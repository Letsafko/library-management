using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books.Abstracts;
using Application.Messaging;
using Domain.Books;
using SharedKernel.Primitives;

namespace Application.Features.Books.AddCopy;

public sealed class AddBookCopyCommandHandler(
    IBookRepository bookRepository,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<AddBookCopyCommand, BookResponse>
{
    public async Task<Result<BookResponse>> HandleAsync(AddBookCopyCommand? request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request!.BookId, cancellationToken);
        if (book is null)
        {
            return BookErrors.BookNotFound;
        }

        var result = book.AddCopy(request.Isbn!, dateTimeProvider.UtcNow);
        if (!result.IsSuccess)
        {
            return result.Error;
        }

        await bookRepository.UpdateAsync(book, cancellationToken);
        return new BookResponse(book);
    }
}
