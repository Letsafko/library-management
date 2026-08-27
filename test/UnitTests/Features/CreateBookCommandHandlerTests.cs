using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Books;
using Application.Features.Books.Abstracts;
using Application.Features.Books.Create;
using Bogus;
using Domain.Books;
using FluentAssertions;
using Infrastructure.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Primitives;
using Xunit;

namespace UnitTests.Features;

public sealed class CreateBookCommandFaker : Faker<CreateBookCommand>
{
    public CreateBookCommandFaker(
        string? title = null,
        string? author = null, 
        string? isbn = null,
        bool forceNullTitle = false,
        bool forceNullAuthor = false,
        bool forceNullIsbn = false)
    {
        CustomInstantiator(f => new CreateBookCommand(
            Title: forceNullTitle ? null : title ?? f.Commerce.ProductName(),
            Author: forceNullAuthor ? null : author ?? f.Name.FullName(),
            Isbn: forceNullIsbn ? null : isbn ?? f.Commerce.Ean13())); 
    }
}

public sealed class CreateBookCommandHandlerTests : CommandHandlerBaseTests<CreateBookCommand, BookResponse>
{
    private readonly LoggingDecorator.RequestHandler<CreateBookCommand, BookResponse> _requestHandler;
    private readonly Mock<IBookRepository> _bookRepository;
    public CreateBookCommandHandlerTests() : base(new CreateBookCommandValidator())
    {
        _bookRepository = new Mock<IBookRepository>();
        var innerHandler = new CreateBookCommandHandler(_bookRepository.Object, DateTimeProvider.Object);
        var validatorHandler = new ValidationDecorator.RequestHandler<CreateBookCommand, BookResponse>(
            innerHandler, 
            Services.Object);
        
        _requestHandler = new LoggingDecorator.RequestHandler<CreateBookCommand, BookResponse>(validatorHandler, Logger.Object);
    }
    
    [Fact]
    public async Task ShouldReturnsAndPersistsBookWhenRequestIsValid()
    {
        // Arrange
        var command = new CreateBookCommandFaker().Generate();

        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(command.Title);
        result.Value.Author.Should().Be(command.Author);
        result.Value.CopyResponses.Should().ContainSingle();
        result.Value.CopyResponses[0].Isbn.Should().Be(command.Isbn);
        result.Value.CopyResponses[0].IsAvailable.Should().BeTrue();

        _bookRepository.Verify(
            x => x.AddAsync(
                It.Is<Book>(b =>
                    b.Title == command.Title &&
                    b.Author == command.Author &&
                    b.CreatedDatetime == DateTime &&
                    b.BookCopies.Count == 1 &&
                    b.BookCopies[0].Isbn.Value == command.Isbn &&
                    b.BookCopies[0].IsAvailable),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        Logger.Verify(x
            => x.Log(LogLevel.Information, $"Processing request {nameof(CreateBookCommand)}", It.IsAny<Exception?>()), Times.Once);
        
        Logger.Verify(x
            => x.Log(LogLevel.Information, $"Completed request {nameof(CreateBookCommand)} successfully.", It.IsAny<Exception?>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsNullValueErrorWhenRequestIsNull()
    {
        // Act
        var result = await _requestHandler.HandleAsync(null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        
        result.Error.Should().Be(ErrorResult.NullValue);
        _bookRepository.Verify(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
        Logger.Verify(x
                => x.Log(
                    LogLevel.Error,
                    $"Completed request {nameof(CreateBookCommand)} with error(s): {{\"Code\":\"General.Null\",\"Description\":\"Null value was provided\",\"Type\":\"Failure\"}}", 
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
        var command = new CreateBookCommandFaker(isbn: isbn).Generate();

        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookErrors.InvalidIsbnLength);
        _bookRepository.Verify(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }
        
    [Theory]
    [MemberData(nameof(CreateBookCommandDataSetup))]
    public async Task ShouldReturnsMissingBookTitleErrorWhenTitleIsNull(CreateBookCommand command, ErrorResult error)
    {
        // Act
        var result = await _requestHandler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(error));
        _bookRepository.Verify(x
                => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
        
    public static TheoryData<CreateBookCommand, ErrorResult> CreateBookCommandDataSetup()
    {
        return new TheoryData<CreateBookCommand, ErrorResult>
        {
            { new CreateBookCommandFaker(title: null, forceNullTitle: true).Generate(), BookErrors.MissingBookTitle },
            { new CreateBookCommandFaker(author: null, forceNullAuthor: true).Generate(), BookErrors.MissingBookAuthor },
            { new CreateBookCommandFaker(isbn: null, forceNullIsbn: true).Generate(), BookErrors.MissingBookIsbn }
        };
    }
}