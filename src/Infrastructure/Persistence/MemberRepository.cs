using System.Threading;
using System.Threading.Tasks;
using Application;
using Domain.Books;
using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class MemberRepository(ApplicationDbContext context) : IMemberRepository
{
    public async Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await context.Members
            .Include(m => m.Loans)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(Member member, BookCopy bookCopy, CancellationToken cancellationToken)
    {
        context.Update(member);
        context.Update(bookCopy);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Member member, CancellationToken cancellationToken)
    {
        context.Update(member);
        await context.SaveChangesAsync(cancellationToken);
    }
}
