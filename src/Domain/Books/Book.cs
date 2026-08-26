using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Domain.Books.ValueObjects;
using SharedKernel;
using SharedKernel.Primitives;

namespace Domain.Books;

public sealed class Book : Entity<int>
{
    private readonly List<BookCopy> _bookCopies;
    private Book(
        Isbn isbn,
        string title,
        string author,
        DateTimeOffset createdDatetime,
        DateTimeOffset lastModifiedDatetime) : base(createdDatetime, lastModifiedDatetime)
    {
        _bookCopies = [];
        Author = author;
        Title = title;
        Isbn = isbn;
    }
    
    public static Result<Book> Create(
        string isbn,
        string title,
        string author,
        DateTimeOffset createdDatetime,
        int numberOfCopies = 1)
    {
        var isbnResult = Isbn.Create(isbn);
        if (!isbnResult.IsSuccess)
        {
            return isbnResult.Error;
        }

        var book = new Book(isbnResult.Value, title, author, createdDatetime, createdDatetime);
        for (var i = 0; i < numberOfCopies; i++)
        {
            book.AddCopy();
        }
        
        return book;
    }
    
    public IReadOnlyList<BookCopy> BookCopies => _bookCopies;
    
    public int AvailableCopiesCount => _bookCopies.Count(c => c.IsAvailable);
    
    public int TotalCopiesCount => _bookCopies.Count;
    public string Author { get; private set; }
    
    public string Title { get; private set; }
    public Isbn Isbn { get; private set; }
    
    private void AddCopy()
    {
        var bookCopy = BookCopy.Create(book: this, DateTimeOffset.UtcNow);
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
