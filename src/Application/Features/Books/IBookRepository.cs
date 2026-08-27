using System.Threading;
using System.Threading.Tasks;
using Domain.Books;

namespace Application.Features.Books.Abstracts;

public interface IBookRepository
{
    Task AddAsync(Book book, CancellationToken cancellationToken);
    Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task UpdateAsync(Book book, CancellationToken cancellationToken);
}