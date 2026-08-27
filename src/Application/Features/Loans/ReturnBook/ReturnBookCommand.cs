using Application.Messaging;

namespace Application.Features.Loans.ReturnBook;

public sealed record ReturnBookCommand(int MemberId, int BookCopyId) : ICommand;
