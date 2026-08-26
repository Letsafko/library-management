using System;
using SharedKernel.Primitives;

namespace Domain.Books;

public sealed class BookCopy : Entity<int>
{
    private BookCopy(
        int bookId,
        bool isAvailable,
        DateTime createdDatetime,
        DateTime lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        IsAvailable = isAvailable;
        BookId = bookId;
    }

    public int BookId { get; private set; }
    public Book Book { get; private set; } = null!;
    public bool IsAvailable { get; private set; }
    
    internal static BookCopy Create(int bookId, DateTime createdDatetime)
    {
        return new BookCopy(bookId, true, createdDatetime, createdDatetime);
    }
    
    internal Result MarkAsBorrowed()
    {
        if (!IsAvailable)
        {
            return BookErrors.AlreadyBorrowed;
        }

        IsAvailable = false;
        return Result.Success();
    }
    
    internal Result MarkAsReturned()
    {
        if (IsAvailable)
        {
            return BookErrors.CopyAlreadyReturned;
        }

        IsAvailable = true;
        return Result.Success();
    }
}