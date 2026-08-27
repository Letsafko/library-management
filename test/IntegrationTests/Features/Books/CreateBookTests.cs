using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Endpoints.Books.Create;
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
public sealed class CreateBookTests(ApplicationFixture fixture)
{
    [Fact]
    public async Task ShouldCreatesAndPersistsBookWhenRequestIsValid()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var response = await fixture.Factory.CreateClient().PostAsJsonAsync("/api/v1/books", request);
        var result = await response.Content.ReadFromJsonAsync<BookViewModel>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var books = await fixture.DbContext.Books
            .Include(x => x.BookCopies)
            .FirstAsync(x => x.Id == result!.Id);
        
        books.Should().NotBeNull();
        books!.Title.Should().Be(result!.Title);
        books.Author.Should().Be(result.Author);
        books.BookCopies.Should().ContainSingle();
        books.BookCopies[0].Isbn.Value.Should().Be(result.Copies[0].Isbn);
        books.BookCopies[0].IsAvailable.Should().Be(result.Copies[0].IsAvailable);
    }

    [Theory]
    [MemberData(nameof(CreateBookRequestDataSetup))]
    public async Task ShouldReturnsBadRequestWhenRequestIsInvalid(Request request, ErrorResult expectedError)
    {
        // Act
        var response = await fixture.Factory.CreateClient().PostAsJsonAsync("/api/v1/books", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problemDetails!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([expectedError]);
    }
    
    public static TheoryData<Request, ErrorResult> CreateBookRequestDataSetup()
    {
        return new TheoryData<Request, ErrorResult>
        {
            { CreateRequest(forceNullAuthor: true), BookErrors.MissingBookAuthor },
            { CreateRequest(forceNullTitle: true), BookErrors.MissingBookTitle },
            { CreateRequest(forceNullIsbn: true), BookErrors.MissingBookIsbn }
        };
    }
    
    private static Request CreateRequest(
        bool forceNullTitle = false,
        bool forceNullAuthor = false,
        bool forceNullIsbn = false)
    {
        return new Faker<Request>()
            .RuleFor(x => x.Isbn, f => forceNullIsbn ? null : f.Commerce.Ean13())
            .RuleFor(x => x.Title, f => forceNullTitle ? null : f.Commerce.ProductName())
            .RuleFor(x => x.Author, f => forceNullAuthor ? null : f.Name.FullName())
            .Generate();
    }
}