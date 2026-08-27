using Application.Messaging;

namespace Application.Features.Loans.BorrowBook;

public sealed record BorrowBookCommand(int MemberId, int BookCopyId) : ICommand;
