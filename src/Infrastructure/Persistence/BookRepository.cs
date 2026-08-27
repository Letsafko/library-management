using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books.Abstracts;
using Domain.Books;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class BookRepository(ApplicationDbContext context) : IBookRepository
{
    public async Task AddAsync(Book book, CancellationToken cancellationToken)
    {
        context.Add(book);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Books
            .Include(b => b.BookCopies)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Book book, CancellationToken cancellationToken)
    {
        context.Update(book);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BookCopy?> GetBookCopyByIdAsync(int bookCopyId, CancellationToken cancellationToken)
    {
        return await context.BookCopies
            .FirstOrDefaultAsync(bc => bc.Id == bookCopyId, cancellationToken);
    }
}