using Application.Messaging;

namespace Application.Features.Books.Returns;

public sealed record ReturnBookCommand(int MemberId, int BookCopyId) : ICommand;
