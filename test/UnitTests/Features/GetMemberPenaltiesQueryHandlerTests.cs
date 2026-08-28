using System.Threading;
using System.Threading.Tasks;
using Application;
using Application.Features.Members.GetPenalties;
using Application.Features.Models;
using Domain.Members;
using Domain.Members.ValueObjects;
using FluentAssertions;
using Infrastructure.Behaviors;
using Moq;
using SharedKernel.Primitives;
using Support.SharedTests.Fakers;
using Xunit;

namespace UnitTests.Features;

public sealed class GetMemberPenaltiesQueryHandlerTests : CommandHandlerBaseTests<GetMemberPenaltiesQuery, MemberPenaltiesResponse>
{
    private readonly LoggingDecorator.RequestHandler<GetMemberPenaltiesQuery, MemberPenaltiesResponse> _handler;
    private readonly Mock<IMemberRepository> _memberRepository;

    public GetMemberPenaltiesQueryHandlerTests() : base(new GetMemberPenaltiesQueryValidator())
    {
        _memberRepository = new Mock<IMemberRepository>();
        var innerHandler = new GetMemberPenaltiesQueryHandler(
            _memberRepository.Object,
            DateTimeProvider.Object);

        var validatorHandler = new ValidationDecorator.RequestHandler<GetMemberPenaltiesQuery, MemberPenaltiesResponse>(
            innerHandler,
            Services.Object);

        _handler = new LoggingDecorator.RequestHandler<GetMemberPenaltiesQuery, MemberPenaltiesResponse>(
            validatorHandler, Logger.Object);
    }

    [Fact]
    public async Task ShouldReturnsZeroWhenMemberHasNoLoans()
    {
        // Arrange
        var member = new MemberFaker(
            id: 1,
            membershipType: MembershipType.Standard,
            createdDatetime: DateTime).Generate();
        
        var query = new GetMemberPenaltiesQuery(member.Id);

        _memberRepository
            .Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MemberId.Should().Be(1);
        result.Value.TotalPendingPenalty.Should().Be(0m);
    }

    [Fact]
    public async Task ShouldReturnsZeroWhenAllActiveLoansAreWithinDueDate()
    {
        // Arrange
        var member = new MemberFaker(
            id: 1,
            membershipType: MembershipType.Standard,
            createdDatetime: DateTime).Generate();
        
        var book = new BookFaker(
            id: 1,
            bookCopyId: 1,
            createdDatetime: DateTime).Generate();
        
        member.BorrowBook(book.BookCopies[0], DateTime);
        var query = new GetMemberPenaltiesQuery(member.Id);

        _memberRepository.Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>())).ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPendingPenalty.Should().Be(0m);
    }

    [Fact]
    public async Task ShouldReturnsPenaltyForSingleOverdueLoan()
    {
        // Arrange
        var borrowedAt = DateTime.AddDays(-30);
        var member = new MemberFaker(
            id: 1,
            membershipType: MembershipType.Standard,
            createdDatetime: DateTime).Generate();
        
        var book = new BookFaker(id: 1, bookCopyId: 1, createdDatetime: DateTime).Generate();
        
        member.BorrowBook(book.BookCopies[0], borrowedAt);
        var query = new GetMemberPenaltiesQuery(MemberId: 1);

        _memberRepository
            .Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPendingPenalty.Should().Be(1.80m); // 9 days * 0.20€
    }

    [Fact]
    public async Task ShouldReturnsTotalPenaltyForMultipleOverdueLoans()
    {
        // Arrange
        var borrowedAt = DateTime.AddDays(-30);
        var member = new MemberFaker(
            id: 1,
            membershipType: MembershipType.Standard,
            createdDatetime: DateTime).Generate();
        
        var book1 = new BookFaker(
            id: 1,
            bookCopyId: 1,
            createdDatetime: DateTime).Generate();
        
        var book2 = new BookFaker(
            id: 2,
            bookCopyId: 2,
            createdDatetime: DateTime).Generate();
        
        member.BorrowBook(book1.BookCopies[0], borrowedAt);
        member.BorrowBook(book2.BookCopies[0], borrowedAt);
        
        var query = new GetMemberPenaltiesQuery(MemberId: 1);

        _memberRepository.Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>())).ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPendingPenalty.Should().Be(3.60m);
    }

    [Fact]
    public async Task ShouldReturnsPenaltyCappedAt10EurosPerLoan()
    {
        // Arrange
        var borrowedAt = DateTime.AddDays(-100);
        var member = new MemberFaker(
            id: 1,
            membershipType: MembershipType.Standard,
            createdDatetime: DateTime).Generate();
        
        var book = new BookFaker(
            id: 1,
            bookCopyId: 1,
            createdDatetime: DateTime).Generate();
        
        member.BorrowBook(book.BookCopies[0], borrowedAt);
        var query = new GetMemberPenaltiesQuery(MemberId: 1);

        _memberRepository.Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>())).ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPendingPenalty.Should().Be(10m);
    }

    [Fact]
    public async Task ShouldExcludesReturnedLoansFromPendingPenalties()
    {
        // Arrange
        var borrowedAt = DateTime.AddDays(-30);
        var member = new MemberFaker(
            id: 1,
            membershipType: MembershipType.Standard,
            createdDatetime: DateTime).Generate();
        
        var book1 = new BookFaker(
            id: 1,
            bookCopyId: 1,
            createdDatetime: DateTime).Generate();
        
        var book2 = new BookFaker(
            id: 2,
            bookCopyId: 2,
            createdDatetime: DateTime).Generate();

        member.BorrowBook(book1.BookCopies[0], borrowedAt);
        member.ReturnBook(book1.BookCopies[0].Id, DateTime); // returned, should not count

        member.BorrowBook(book2.BookCopies[0], DateTime); // borrowed today, no penalty
        var query = new GetMemberPenaltiesQuery(MemberId: 1);

        _memberRepository.Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>())).ReturnsAsync(member);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPendingPenalty.Should().Be(0m);
    }

    [Fact]
    public async Task ShouldReturnsMemberNotFoundWhenMemberDoesNotExist()
    {
        // Arrange
        var query = new GetMemberPenaltiesQuery(MemberId: 999);
        _memberRepository
            .Setup(x => x.GetByIdAsync(query.MemberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Member?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MemberErrors.MemberNotFound);
    }

    [Fact]
    public async Task ShouldReturnsMissingFieldErrorWhenMemberIdIsInvalid()
    {
        // Act
        var result = await _handler.HandleAsync(new GetMemberPenaltiesQuery(MemberId: 0), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(MemberErrors.InvalidMemberId));

        _memberRepository
            .Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
