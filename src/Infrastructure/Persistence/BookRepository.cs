using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application;
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

    public async Task<Book?> GetBookByBookCopyIdAsync(int bookCopyId, CancellationToken cancellationToken)
    {
        return await context.Books
            .Include(b => b.BookCopies.Where(bc => bc.Id == bookCopyId))
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.BookCopies.Any(bc => bc.Id == bookCopyId), cancellationToken);
    }
}