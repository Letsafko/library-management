using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Books.ValueObjects;
using SharedKernel.Primitives;

namespace Domain.Books;

public sealed class Book : Entity<int>
{
    private readonly List<BookCopy> _bookCopies;
    private Book(
        string title,
        string author,
        DateTime createdDatetime,
        DateTime lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        _bookCopies = [];
        Author = author;
        Title = title;
    }
    
    public static Result<Book> Create(
        string isbn,
        string title,
        string author,
        DateTime createdDatetime)
    {
        var book = new Book(title, author, createdDatetime, createdDatetime);
        var isbnResult = Isbn.Create(isbn);
        if (!isbnResult.IsSuccess)
        {
            return isbnResult.Error;
        }
        
        book.AddCopy(isbnResult.Value, createdDatetime);
        return book;
    }
    
    public IReadOnlyList<BookCopy> BookCopies => _bookCopies;
    public int AvailableCopiesCount => _bookCopies.Count(c => c.IsAvailable);
    public int TotalCopiesCount => _bookCopies.Count;
    public string Author { get; private set; }
    public string Title { get; private set; }
    
    private void AddCopy(Isbn isbn, DateTime currentDatetime)
    {
        var bookCopy = BookCopy.Create(bookId: Id, isbn, currentDatetime);
        _bookCopies.Add(bookCopy);
    }
    
    public Result MarkCopyAsBorrowed(int bookCopyId)
    {
        var bookCopy = _bookCopies.FirstOrDefault(c => c.Id == bookCopyId);
        return bookCopy is null ? BookErrors.CopyNotFound : bookCopy.MarkAsBorrowed();
    }
    
    public Result MarkCopyAsReturned(int bookCopyId)
    {
        var copy = _bookCopies.FirstOrDefault(c => c.Id == bookCopyId);
        return copy is null ? BookErrors.CopyNotFound : copy.MarkAsReturned();
    }
}
