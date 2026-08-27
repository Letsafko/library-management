using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Books.ValueObjects;
using SharedKernel.Primitives;

namespace Domain.Books;

public class Book : Entity<int>
{
    private readonly List<BookCopy> _bookCopies;
    protected Book(
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
        var result = book.AddCopy(isbn, createdDatetime);
        if (!result.IsSuccess)
        {
            return result.Error;
        }
        
        return book;
    }
    
    public IReadOnlyList<BookCopy> BookCopies => _bookCopies;
    public int AvailableCopiesCount => _bookCopies.Count(c => c.IsAvailable);
    public int TotalCopiesCount => _bookCopies.Count;
    public string Author { get; private set; }
    public string Title { get; private set; }
    public Result AddCopy(string isbn, DateTime currentDatetime, int bookCopyId = 0)
    {
        var isbnResult = Isbn.Create(isbn);
        if (!isbnResult.IsSuccess)
        {
            return isbnResult.Error;
        }
        var bookCopy = BookCopy.Create(bookCopyId, bookId: Id, isbnResult.Value, currentDatetime);
        _bookCopies.Add(bookCopy);
        return Result.Success();
    }
    public Result MarkCopyAsReturned(int bookCopyId)
    {
        var bookCopyResult = FindBookCopy(bookCopyId);
        return bookCopyResult.IsSuccess ? bookCopyResult.Value.MarkAsReturned() : bookCopyResult.Error;
    }
    private Result<BookCopy> FindBookCopy(int bookCopyId)
    {
        var bookCopy = _bookCopies.FirstOrDefault(c => c.Id == bookCopyId);
        return bookCopy is null ? BookErrors.CopyNotFound : bookCopy;
    }
}
