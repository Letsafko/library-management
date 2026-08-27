using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books;
using Application.Features.Loans;
using Application.Features.Loans.ReturnBook;
using Domain.Books;
using Domain.Members;
using Domain.Members.ValueObjects;
using FluentAssertions;
using Infrastructure.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Primitives;
using Support.SharedTests.Fakers;
using Xunit;

namespace UnitTests.Features;

public sealed class ReturnBookCommandHandlerTests : CommandHandlerBaseTests<ReturnBookCommand, ReturnBookResponse>
{
    private readonly LoggingDecorator.RequestHandler<ReturnBookCommand, ReturnBookResponse> _handler;
    private readonly Mock<IMemberRepository> _memberRepository;
    private readonly Mock<IBookRepository> _bookRepository;
    public ReturnBookCommandHandlerTests() : base(new ReturnBookCommandValidator())
    {
        _memberRepository = new Mock<IMemberRepository>();
        _bookRepository = new Mock<IBookRepository>();
        var innerHandler = new ReturnBookCommandHandler(
            _memberRepository.Object,
            _bookRepository.Object,
            DateTimeProvider.Object);

        var validatorHandler = new ValidationDecorator.RequestHandler<ReturnBookCommand, ReturnBookResponse>(
            innerHandler,
            Services.Object);

        _handler = new LoggingDecorator.RequestHandler<ReturnBookCommand, ReturnBookResponse>(
            validatorHandler, Logger.Object);
    }

    [Fact]
    public async Task ShouldReturnsZeroPenaltyWhenBookIsReturnedOnTime()
    {
        // Arrange
        var member = new MemberFaker(id: 1, membershipType: MembershipType.Standard, createdDatetime: DateTime).Generate();
        var book = new BookFaker(id: 1, bookCopyId: 1, createdDatetime: DateTime).Generate();
        var bookCopy = book.BookCopies[0];
        member.BorrowBook(bookCopy, DateTime);
        _memberRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(member);
        _bookRepository.Setup(x => x.GetBookByBookCopyIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(book);
        var command = new ReturnBookCommand(member.Id, bookCopy.Id);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DaysLate.Should().Be(0);
        result.Value.PenaltyAmount.Should().Be(0m);
        result.Value.ReturnedAt.Should().Be(DateTime);
        result.Value.BookCopyId.Should().Be(bookCopy.Id);

        _memberRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Member>(m => m.Loans[0].IsReturned),
                It.IsAny<CancellationToken>()), Times.Once);

        _bookRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Book>(b => b.BookCopies[0].IsAvailable),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsPenaltyAndDaysLateWhenBookIsReturnedLate()
    {
        // Arrange
        var borrowedAt = DateTime.AddDays(-30);
        var member = new MemberFaker(id: 1, membershipType: MembershipType.Standard, createdDatetime: DateTime).Generate();
        var book = new BookFaker(id: 1, bookCopyId: 1, createdDatetime: DateTime).Generate();
        var bookCopy = book.BookCopies[0];

        member.BorrowBook(bookCopy, borrowedAt);

        _memberRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(member);
        _bookRepository.Setup(x => x.GetBookByBookCopyIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var command = new ReturnBookCommand(member.Id, bookCopy.Id);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DaysLate.Should().Be(9);
        result.Value.PenaltyAmount.Should().Be(1.80m);
        result.Value.ReturnedAt.Should().Be(DateTime);
    }

    [Fact]
    public async Task ShouldReturnsPenaltyCappedAt10EurosWhenRetardIsVeryLong()
    {
        // Arrange
        var borrowedAt = DateTime.AddDays(-100);
        var member = new MemberFaker(id: 1, membershipType: MembershipType.Standard, createdDatetime: DateTime).Generate();
        var book = new BookFaker(id: 1, bookCopyId: 1, createdDatetime: DateTime).Generate();
        var bookCopy = book.BookCopies[0];

        member.BorrowBook(bookCopy, borrowedAt);

        _memberRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(member);
        _bookRepository.Setup(x => x.GetBookByBookCopyIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var command = new ReturnBookCommand(member.Id, bookCopy.Id);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DaysLate.Should().Be(79);
        result.Value.PenaltyAmount.Should().Be(10m);
    }

    [Fact]
    public async Task ShouldReturnsMemberNotFoundErrorWhenMemberDoesNotExist()
    {
        // Arrange
        var command = new ReturnBookCommand(MemberId: 999, BookCopyId: 1);

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MemberErrors.MemberNotFound);
        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnsLoanNotFoundErrorWhenNoActiveLoanExistsForBookCopy()
    {
        // Arrange
        var member = new MemberFaker(id: 1, membershipType: MembershipType.Standard, createdDatetime: DateTime).Generate();
        var command = new ReturnBookCommand(MemberId: 1, BookCopyId: 1);

        _memberRepository
            .Setup(x => x.GetByIdAsync(command.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MemberErrors.LoanNotFound);
        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookRepository.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldReturnsNullValueErrorWhenRequestIsNull()
    {
        // Act
        var result = await _handler.HandleAsync(null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorResult.NullValue);

        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Never);
        Logger.Verify(x
                => x.Log(
                    LogLevel.Error,
                    $"Completed request {nameof(ReturnBookCommand)} with error(s): {{\"Code\":\"General.Null\",\"Description\":\"Null value was provided\",\"Type\":\"Failure\"}}",
                    It.IsAny<Exception?>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(ReturnBookCommandDataSetup))]
    public async Task ShouldReturnsMissingFieldErrorWhenCommandIsInvalid(ReturnBookCommand command, ErrorResult error)
    {
        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(error));
        _memberRepository.Verify(x => x.UpdateAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static TheoryData<ReturnBookCommand, ErrorResult> ReturnBookCommandDataSetup()
    {
        return new TheoryData<ReturnBookCommand, ErrorResult>
        {
            { new ReturnBookCommand(MemberId: 0, BookCopyId: 1), MemberErrors.InvalidMemberId },
            { new ReturnBookCommand(MemberId: 1, BookCopyId: 0), BookErrors.InvalidBookCopyId }
        };
    }
}
