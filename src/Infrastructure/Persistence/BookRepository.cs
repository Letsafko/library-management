using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books.Abstracts;
using Domain.Books;

namespace Infrastructure.Persistence;

public sealed class BookRepository(ApplicationDbContext context) : IBookRepository
{
    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        context.Add(book);
        await context.SaveChangesAsync(cancellationToken);
    }
}