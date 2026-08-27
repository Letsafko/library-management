using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Endpoints.Books.AddCopy;
using Api.ViewModels;
using Bogus;
using Domain.Books;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Primitives;
using Xunit;

namespace IntegrationTests.Features.Books;

[Collection(ApplicationCollectionFixture.Name)]
public sealed class AddBookCopyTests(ApplicationFixture fixture)
{
    [Fact]
    public async Task ShouldCreatesAndPersistsBookCopyWhenRequestIsValid()
    {
        // Arrange
        var createdBook = await CreateBookAsync();
        var request = CreateRequest();

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/books/{createdBook!.Id}/copies", request);
        var result = await response.Content.ReadFromJsonAsync<BookViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var book = await fixture.DbContext.Books
            .Include(b => b.BookCopies)
            .AsTracking()
            .FirstAsync(b => b.Id == createdBook.Id);

        book.BookCopies.Should().HaveCount(2);
        book.BookCopies.Should().ContainSingle(c => c.Isbn.Value == request.Isbn);

        result!.Copies.Should().HaveCount(2);
        result.Copies.Should().ContainSingle(c => c.Isbn == request.Isbn && c.IsAvailable);
    }

    [Fact]
    public async Task ShouldReturnsNotFoundWhenBookDoesNotExist()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/books/99999/copies", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(AddBookCopyRequestDataSetup))]
    public async Task ShouldReturnsBadRequestWhenRequestIsInvalid(int bookId, Request request, ErrorResult expectedError)
    {
        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/books/{bookId}/copies", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problemDetails!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([expectedError]);
    }

    public static TheoryData<int, Request, ErrorResult> AddBookCopyRequestDataSetup()
    {
        return new TheoryData<int, Request, ErrorResult>
        {
            { 1, CreateRequest(forceNullIsbn: true), BookErrors.MissingBookIsbn },
            { 0, CreateRequest(), BookErrors.InvalidBookId }
        };
    }

    private async Task<BookViewModel?> CreateBookAsync()
    {
        var createRequest = new Faker<Api.Endpoints.Books.Create.Request>()
            .RuleFor(x => x.Isbn, f => f.Commerce.Ean13())
            .RuleFor(x => x.Title, f => f.Commerce.ProductName())
            .RuleFor(x => x.Author, f => f.Name.FullName())
            .Generate();

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/books", createRequest);
        return await response.Content.ReadFromJsonAsync<BookViewModel>();
    }

    private static Request CreateRequest(bool forceNullIsbn = false)
    {
        return new Faker<Request>()
            .RuleFor(x => x.Isbn, f => forceNullIsbn ? null : f.Commerce.Ean13())
            .Generate();
    }
}
