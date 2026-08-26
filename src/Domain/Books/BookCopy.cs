using System;
using SharedKernel.Primitives;

namespace Domain.Books;

public sealed class BookCopy : Entity<int>
{
    private BookCopy(
        bool isAvailable,
        Book book,
        DateTimeOffset createdDatetime,
        DateTimeOffset lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        IsAvailable = isAvailable;
        Book = book;
    }

    public Book Book { get; private set; }
    public bool IsAvailable { get; private set; }
    
    internal static BookCopy Create(Book book, DateTimeOffset createdDatetime)
    {
        return new BookCopy(
            isAvailable: true,
            book,
            createdDatetime,
            createdDatetime);
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