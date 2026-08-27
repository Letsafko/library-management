using System.Threading;
using System.Threading.Tasks;
using Domain.Books;

namespace Application.Features.Books;

public interface IBookRepository
{
    Task AddAsync(Book book, CancellationToken cancellationToken);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task UpdateAsync(Book book, CancellationToken cancellationToken);
    Task<BookCopy?> GetBookCopyByIdAsync(int bookCopyId, CancellationToken cancellationToken);
    Task<Book?> GetBookByBookCopyIdAsync(int bookCopyId, CancellationToken cancellationToken);
}