using System.Collections.Generic;
using System.Linq;
using Application.Features.Books;

namespace Api.ViewModels;

public sealed record BookViewModel
{
    public BookViewModel(){}
    
    public BookViewModel(BookResponse bookResponse)
    {
        Id = bookResponse.BookId;
        Author = bookResponse.Author;
        Title = bookResponse.Title;
        Copies = bookResponse
            .CopyResponses
            .Select(x => new BookCopyViewModel(x))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<BookCopyViewModel> Copies { get; init; } = [];
    public string Author { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public int Id { get; init; }
}

public sealed record BookCopyViewModel
{
    public BookCopyViewModel(){}
    
    public BookCopyViewModel(BookCopyResponse bookCopyResponse)
    {
        IsAvailable = bookCopyResponse.IsAvailable;
        Isbn = bookCopyResponse.Isbn;
    }
    public string Isbn { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
}
