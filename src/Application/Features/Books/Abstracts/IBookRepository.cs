using System.Threading;
using System.Threading.Tasks;
using Domain.Books;

namespace Application.Features.Books.Abstracts;

public interface IBookRepository
{
    Task AddAsync(Book book, CancellationToken cancellationToken);
}