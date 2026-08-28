using System.Threading;
using System.Threading.Tasks;
using Domain.Books;
using Domain.Members;

namespace Application;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task UpdateAsync(Member member, BookCopy bookCopy, CancellationToken cancellationToken);
    Task UpdateAsync(Member member, CancellationToken cancellationToken);
}
