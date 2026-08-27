using Application.Messaging;

namespace Application.Features.Books.Create;

public sealed record CreateBookCommand(
    string? Title,
    string? Author,
    string? Isbn) :  ICommand;