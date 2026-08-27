using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books;
using Application.Features.Books.Abstracts;
using Application.Features.Books.AddCopy;
using Application.Messaging;
using Bogus;
using Domain.Books;
using FluentAssertions;
using FluentValidation;
using Infrastructure.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Primitives;
using Xunit;

namespace UnitTests.Features;

public sealed class AddBookCopyCommandFaker : Faker<AddBookCopyCommand>
{
    public AddBookCopyCommandFaker(
        int? bookId = null,
        string? isbn = null,
        bool forceNullIsbn = false)
    {
        CustomInstantiator(f => new AddBookCopyCommand(
            BookId: bookId ?? f.Random.Int(1, 1000),
            Isbn: forceNullIsbn ? null : isbn ?? f.Commerce.Ean13()));
    }
}

public sealed class AddBookCopyCommandHandlerTests
{
    private readonly Mock<MockLogger<IRequestHandler<AddBookCopyCommand, BookResponse>>> _logger;
#pragma warning disable CA1859
    private readonly IRequestHandler<AddBookCopyCommand, BookResponse> _requestHandler;
#pragma warning restore CA1859
    private readonly Mock<IBookRepository> _bookRepository;

    private static readonly DateTime DateTime = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public AddBookCopyCommandHandlerTests()
    {
        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.SetupGet(x => x.UtcNow).Returns(DateTime);

        _bookRepository = new Mock<IBookRepository>();

        var services = new Mock<IServiceProvider>();
        services
            .Setup(x => x.GetService(typeof(IValidator<AddBookCopyCommand>)))
            .Returns(new AddBookCopyCommandValidator());

        var handler = new AddBookCopyCommandHandler(_bookRepository.Object, dateTimeProvider.Object);

        var validatorHandler = new ValidationDecorator.RequestHandler<AddBookCopyCommand, BookResponse>(
            handler,
            services.Object);

        _logger = new Mock<MockLogger<IRequestHandler<AddBookCopyCommand, BookResponse>>>();
        _requestHandler = new LoggingDecorator.RequestHandler<AddBookCopyCommand, BookResponse>(validatorHandler, _logger.Object);
    }

    [Fact]
    public async Task ShouldReturnsAndPersistsBookCopyWhenRequestIsValid()
    {
        // Arrange
        const string existingIsbn = "1234567890";
        var existingBook = Book.Create(existingIsbn, "Clean Code", "Robert Martin", DateTime).Value;
        var command = new AddBookCopyCommandFaker(bookId: 1).Generate();

        _bookRepository
            .Setup(x => x.GetByIdAsync(command.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CopyResponses.Should().HaveCount(2);
        result.Value.CopyResponses[1].Isbn.Should().Be(command.Isbn);
        result.Value.CopyResponses[1].IsAvailable.Should().BeTrue();

        _bookRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Book>(b =>
                    b.BookCopies.Count == 2 &&
                    b.BookCopies[1].Isbn.Value == command.Isbn &&
                    b.BookCopies[1].IsAvailable),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _logger.Verify(x
            => x.Log(LogLevel.Information, $"Processing request {nameof(AddBookCopyCommand)}", It.IsAny<Exception?>()), Times.Once);

        _logger.Verify(x
            => x.Log(LogLevel.Information, $"Completed request {nameof(AddBookCopyCommand)} successfully.", It.IsAny<Exception?>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsNullValueErrorWhenRequestIsNull()
    {
        // Act
        var result = await _requestHandler.HandleAsync(null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorResult.NullValue);

        _bookRepository.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(x
                => x.Log(
                    LogLevel.Error,
                    $"Completed request {nameof(AddBookCopyCommand)} with error(s): {{\"Code\":\"General.Null\",\"Description\":\"Null value was provided\",\"Type\":\"Failure\"}}",
                    It.IsAny<Exception?>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsBookNotFoundErrorWhenBookDoesNotExist()
    {
        // Arrange
        var command = new AddBookCopyCommandFaker(bookId: 999).Generate();

        _bookRepository
            .Setup(x => x.GetByIdAsync(command.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookErrors.BookNotFound);

        _bookRepository.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(x
                => x.Log(
                    LogLevel.Error,
                    $"Completed request {nameof(AddBookCopyCommand)} with error(s): {{\"Code\":\"Book.NotFound\",\"Description\":\"Book not found.\",\"Type\":\"NotFound\"}}",
                    It.IsAny<Exception?>()),
            Times.Once);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("12345")]
    [InlineData("123456")]
    [InlineData("1234567")]
    [InlineData("12345678")]
    [InlineData("123456789")]
    [InlineData("12345678901234")]
    [InlineData("123456789012345")]
    [InlineData("1234567890123456")]
    [InlineData("12345678901234567")]
    [InlineData("123456789012345678")]
    public async Task ShouldReturnsInvalidIsbnLengthErrorWhenIsbnHasInvalidLength(string isbn)
    {
        // Arrange
        var existingBook = Book.Create("1234567890", "Clean Code", "Robert Martin", DateTime).Value;
        var command = new AddBookCopyCommandFaker(bookId: 1, isbn: isbn).Generate();
        

        _bookRepository
            .Setup(x => x.GetByIdAsync(command.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookErrors.InvalidIsbnLength);
        _bookRepository.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(AddBookCopyCommandDataSetup))]
    public async Task ShouldReturnsMissingFieldErrorWhenCommandIsInvalid(AddBookCopyCommand command, ErrorResult error)
    {
        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(error));

        _bookRepository.Verify(x
                => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    public static TheoryData<AddBookCopyCommand, ErrorResult> AddBookCopyCommandDataSetup()
    {
        return new TheoryData<AddBookCopyCommand, ErrorResult>
        {
            { new AddBookCopyCommandFaker(bookId: 0).Generate(), BookErrors.InvalidBookId },
            { new AddBookCopyCommandFaker(isbn: null, forceNullIsbn: true).Generate(), BookErrors.MissingBookIsbn }
        };
    }
}
