using Application.Messaging;

namespace Application.Features.Books.Borrow;

public sealed record BorrowBookCommand(int MemberId, int BookCopyId) : ICommand;
