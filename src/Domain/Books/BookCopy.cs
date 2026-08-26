using System;
using Domain.Books.ValueObjects;
using SharedKernel.Primitives;

namespace Domain.Books;

public sealed class BookCopy : Entity<int>
{
    private BookCopy(
        int bookId,
        Isbn isbn,
        bool isAvailable,
        DateTime createdDatetime,
        DateTime lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        IsAvailable = isAvailable;
        BookId = bookId;
        Isbn = isbn;
    }

    public int BookId { get; private set; }
    public Isbn Isbn { get; private set; }
    public Book Book { get; private set; } = null!;
    public bool IsAvailable { get; private set; }
    
    internal static BookCopy Create(int bookId, Isbn isbn, DateTime createdDatetime)
    {
        return new BookCopy(bookId, isbn, true, createdDatetime, createdDatetime);
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