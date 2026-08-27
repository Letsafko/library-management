using Application.Messaging;

namespace Application.Features.Books.AddCopy;

public sealed record AddBookCopyCommand(int BookId, string? Isbn) : ICommand;
