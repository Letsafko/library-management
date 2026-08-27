using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Endpoints.Loans.BorrowBook;
using Api.ViewModels;
using Bogus;
using Domain.Books;
using Domain.Members;
using Domain.Members.ValueObjects;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Primitives;
using Support.SharedTests.Fakers;
using Xunit;

namespace IntegrationTests.Features.Loans;

[Collection(ApplicationCollectionFixture.Name)]
public sealed class BorrowBookTests(ApplicationFixture fixture)
{
    private readonly IDateTimeProvider _dateTimeProvider 
        = fixture.Factory.Services.GetRequiredService<IDateTimeProvider>();
    
    [Fact]
    public async Task ShouldCreatesAndPersistsLoanWhenRequestIsValid()
    {
        // Arrange
        
        var member = await SeedMemberAsync();
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();
        var request = new Request { BookCopyId = bookCopyId };

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{member.Id}/loans", request);
        var result = await response.Content.ReadFromJsonAsync<LoanViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.MemberId.Should().Be(member.Id);
        result.BookCopyId.Should().Be(bookCopyId);
        result.DueDate.Should().Be(_dateTimeProvider.UtcNow.Add(member.MembershipType.LoanDuration));

        var loan = await fixture.DbContext.Loans
            .FirstAsync(l => l.MemberId == member.Id && l.BookCopyId == bookCopyId);

        loan.Should().NotBeNull();
        loan.ReturnedAt.Should().BeNull();

        var copy = await fixture.DbContext.BookCopies.FindAsync(bookCopyId);
        copy!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldReturnsNotFoundWhenMemberDoesNotExist()
    {
        // Arrange
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();
        var request = new Request { BookCopyId = bookCopyId };

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/members/99999/loans", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnsNotFoundWhenBookCopyDoesNotExist()
    {
        // Arrange
        var member = await SeedMemberAsync();
        var request = new Request { BookCopyId = 99999 };

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{member.Id}/loans", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnsBadRequestWhenMemberHasReachedStandardLoanLimit()
    {
        // Arrange
        var borrower = await SeedMemberAsync(MembershipType.Standard);

        for (var i = 0; i < MembershipType.Standard.MaxSimultaneousLoans; i++)
        {
            var (_, copyId) = await CreateBookWithCopyIdAsync();
            await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{borrower.Id}/loans", new Request { BookCopyId = copyId });
        }

        var (_, extraCopyId) = await CreateBookWithCopyIdAsync();
        var request = new Request { BookCopyId = extraCopyId };

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{borrower.Id}/loans", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails!.Title.Should().Be(MemberErrors.LoanLimitReached.Code);
    }

    [Fact]
    public async Task ShouldReturnsBadRequestWhenBookCopyIsAlreadyBorrowed()
    {
        // Arrange
        var borrower = await SeedMemberAsync();
        var anotherMember = await SeedMemberAsync();
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();

        // First member borrows the copy
        await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{borrower.Id}/loans", new Request { BookCopyId = bookCopyId });

        // Act — second member tries to borrow the same copy
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{anotherMember.Id}/loans", new Request { BookCopyId = bookCopyId });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails!.Title.Should().Be(BookErrors.AlreadyBorrowed.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidRequestDataSetup))]
    public async Task ShouldReturnsBadRequestWhenRequestIsInvalid(int memberId, Request request, ErrorResult expectedError)
    {
        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync($"/api/v1/members/{memberId}/loans", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problemDetails!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([expectedError]);
    }

    public static TheoryData<int, Request, ErrorResult> InvalidRequestDataSetup()
    {
        return new TheoryData<int, Request, ErrorResult>
        {
            { 0, new Request { BookCopyId = 1 }, MemberErrors.InvalidMemberId },
            { 1, new Request { BookCopyId = 0 }, BookErrors.InvalidBookCopyId }
        };
    }

    private async Task<Member> SeedMemberAsync(MembershipType? membershipType = null)
    {
        var member = new MemberFaker(
            membershipType: membershipType,
            createdDatetime: _dateTimeProvider.UtcNow,
            lastModifiedDatetime: _dateTimeProvider.UtcNow).Generate();

        fixture.DbContext.Members.Add(member);
        await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.ChangeTracker.Clear();
        return member;
    }

    private async Task<(BookViewModel book, int bookCopyId)> CreateBookWithCopyIdAsync()
    {
        var createRequest = new Faker<Api.Endpoints.Books.Create.Request>()
            .RuleFor(x => x.Isbn, f => f.Commerce.Ean13())
            .RuleFor(x => x.Title, f => f.Commerce.ProductName())
            .RuleFor(x => x.Author, f => f.Name.FullName())
            .Generate();

        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/books", createRequest);
        var book = (await response.Content.ReadFromJsonAsync<BookViewModel>())!;

        var copy = await fixture.DbContext.BookCopies.FirstAsync(bc => bc.BookId == book.Id);

        return (book, copy.Id);
    }
}
