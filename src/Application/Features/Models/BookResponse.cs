using System.Collections.Generic;
using System.Linq;
using Domain.Books;

namespace Application.Features.Models;

public sealed record BookResponse
{
    private readonly List<BookCopyResponse> _copyResponses = [];
    public BookResponse(Book book)
    {
        _copyResponses = book.BookCopies.Select(bc => new BookCopyResponse(bc)).ToList();
        Author = book.Author;
        Title = book.Title;
        BookId = book.Id;
    }
    
    public IReadOnlyList<BookCopyResponse> CopyResponses => _copyResponses.AsReadOnly();
    public string Author { get; }
    public string Title { get; }
    public int BookId { get; }
}

public sealed record BookCopyResponse
{
    public BookCopyResponse(BookCopy bookCopy)
    {
        IsAvailable = bookCopy.IsAvailable;
        Isbn = bookCopy.Isbn.Value;
        BookId = bookCopy.BookId;
    }
    public bool IsAvailable { get; } 
    public string Isbn { get; }
    public int BookId { get; }
}