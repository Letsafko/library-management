using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books.Abstracts;
using Application.Features.Loans;
using Application.Features.Loans.BorrowBook;
using Bogus;
using Domain.Books;
using Domain.Members;
using Domain.Members.ValueObjects;
using FluentAssertions;
using Infrastructure.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Primitives;
using Xunit;

namespace UnitTests.Features;

public sealed class BorrowBookCommandFaker : Faker<BorrowBookCommand>
{
    public BorrowBookCommandFaker(int? memberId = null, int? bookCopyId = null)
    {
        CustomInstantiator(f => new BorrowBookCommand(
            MemberId: memberId ?? f.Random.Int(1, 1000),
            BookCopyId: bookCopyId ?? f.Random.Int(1, 1000)));
    }
}

public sealed class BorrowBookCommandHandlerTests : CommandHandlerBaseTests<BorrowBookCommand, LoanResponse>
{
    private readonly LoggingDecorator.RequestHandler<BorrowBookCommand, LoanResponse> _handler;
    private readonly Mock<IMemberRepository> _memberRepository;
    private readonly Mock<IBookRepository> _bookRepository;

    public BorrowBookCommandHandlerTests(): base(new BorrowBookCommandValidator())
    {
        _memberRepository = new Mock<IMemberRepository>();
        _bookRepository = new Mock<IBookRepository>();
        var handler = new BorrowBookCommandHandler(
            _memberRepository.Object,
            _bookRepository.Object,
            DateTimeProvider.Object);

        var validatorHandler = new ValidationDecorator.RequestHandler<BorrowBookCommand, LoanResponse>(
            handler,
            Services.Object);

        _handler = new LoggingDecorator.RequestHandler<BorrowBookCommand, LoanResponse>(validatorHandler, Logger.Object);
    }

    [Fact]
    public async Task ShouldReturnsAndPersistsLoanWhenRequestIsValid()
    {
        // Arrange
        var member = Member.Create("John", "Doe", "john@example.com", MembershipType.Standard, DateTime);
        var book = Book.Create("1234567890", "Clean Code", "Robert Martin", DateTime).Value;
        var bookCopy = book.BookCopies[0];
        var command = new BorrowBookCommandFaker(memberId: 1, bookCopyId: 1).Generate();

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        _bookRepository
            .Setup(x => x.GetBookCopyByIdAsync(command.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MemberId.Should().Be(member.Id);
        result.Value.BookCopyId.Should().Be(bookCopy.Id);
        result.Value.BorrowedAt.Should().Be(DateTime);
        result.Value.DueDate.Should().Be(DateTime.Add(MembershipType.Standard.LoanDuration));

        _memberRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Member>(m => m.ActiveLoansCount == 1 && m.Loans[0].BookCopyId == bookCopy.Id),
                It.Is<BookCopy>(bc => !bc.IsAvailable),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Logger.Verify(x
            => x.Log(LogLevel.Information, $"Processing request {nameof(BorrowBookCommand)}", It.IsAny<Exception?>()), Times.Once);

        Logger.Verify(x
            => x.Log(LogLevel.Information, $"Completed request {nameof(BorrowBookCommand)} successfully.", It.IsAny<Exception?>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsNullValueErrorWhenRequestIsNull()
    {
        // Act
        var result = await _handler.HandleAsync(null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorResult.NullValue);

        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()), Times.Never);
        Logger.Verify(x
                => x.Log(
                    LogLevel.Error,
                    $"Completed request {nameof(BorrowBookCommand)} with error(s): {{\"Code\":\"General.Null\",\"Description\":\"Null value was provided\",\"Type\":\"Failure\"}}",
                    It.IsAny<Exception?>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsMemberNotFoundWhenMemberDoesNotExist()
    {
        // Arrange
        var command = new BorrowBookCommandFaker(memberId: 999).Generate();

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MemberErrors.MemberNotFound);

        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnsBookCopyNotFoundWhenBookCopyDoesNotExist()
    {
        // Arrange
        var member = Member.Create("John", "Doe", "john@example.com", MembershipType.Standard, DateTime);
        var command = new BorrowBookCommandFaker(memberId: 1, bookCopyId: 999).Generate();

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        _bookRepository
            .Setup(x => x.GetBookCopyByIdAsync(command.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookCopy?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookErrors.BookCopyNotFound);

        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("Standard", 3)]
    [InlineData("Student", 5)]
    public async Task ShouldReturnsLoanLimitReachedWhenMemberHasReachedQuota(string membershipTypeName, int maxLoans)
    {
        // Arrange
        var membershipType = MembershipType.GetByName(membershipTypeName);
        var member = Member.Create("John", "Doe", "john@example.com", membershipType, DateTime);

        for (var i = 0; i < maxLoans; i++)
        {
            var existingBook = Book.Create($"123456789{i}", "Title", "Author", DateTime).Value;
            member.BorrowBook(existingBook.BookCopies[0], DateTime);
        }

        var book = Book.Create("9876543210", "New Book", "Author", DateTime).Value;
        var bookCopy = book.BookCopies[0];
        var command = new BorrowBookCommandFaker(memberId: 1, bookCopyId: 2).Generate();

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        _bookRepository
            .Setup(x => x.GetBookCopyByIdAsync(command.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MemberErrors.LoanLimitReached);

        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnsBookNotAvailableWhenBookCopyIsAlreadyBorrowed()
    {
        // Arrange
        var book = Book.Create("1234567890", "Clean Code", "Robert Martin", DateTime).Value;
        var bookCopy = book.BookCopies[0];

        var otherMember = Member.Create("Jane", "Doe", "jane@example.com", MembershipType.Standard, DateTime);
        otherMember.BorrowBook(bookCopy, DateTime);

        var member = Member.Create("John", "Doe", "john@example.com", MembershipType.Standard, DateTime);
        var command = new BorrowBookCommandFaker(memberId: 1, bookCopyId: 1).Generate();

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        _bookRepository
            .Setup(x => x.GetBookCopyByIdAsync(command.BookCopyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bookCopy);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookErrors.AlreadyBorrowed);

        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(BorrowBookCommandDataSetup))]
    public async Task ShouldReturnsMissingFieldErrorWhenCommandIsInvalid(BorrowBookCommand command, ErrorResult error)
    {
        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(error));

        _memberRepository.Verify(x
                => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<BookCopy>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public static TheoryData<BorrowBookCommand, ErrorResult> BorrowBookCommandDataSetup()
    {
        return new TheoryData<BorrowBookCommand, ErrorResult>
        {
            { new BorrowBookCommandFaker(memberId: 0).Generate(), MemberErrors.InvalidMemberId },
            { new BorrowBookCommandFaker(bookCopyId: 0).Generate(), BookErrors.InvalidBookCopyId }
        };
    }
}
