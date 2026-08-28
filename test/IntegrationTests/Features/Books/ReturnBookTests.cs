using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
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
using Xunit;

namespace IntegrationTests.Features.Books;

[Collection(ApplicationCollectionFixture.Name)]
public sealed class ReturnBookTests(ApplicationFixture fixture)
{
    private readonly Faker _faker = new();
    private readonly IDateTimeProvider _dateTimeProvider
        = fixture.Factory.Services.GetRequiredService<IDateTimeProvider>();

    [Fact]
    public async Task ShouldCreatesReturnedLoanAndMakesCopyAvailableWhenReturnedOnTime()
    {
        // Arrange
        var member = await SeedMemberAsync();
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();

        await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/members/{member.Id}/loans",
            new { bookCopyId });

        // Act
        var response = await fixture.HttpClient.PostAsync(
            new Uri($"/api/v1/members/{member.Id}/loans/{bookCopyId}/return", UriKind.Relative),
            content: null);

        var result = await response.Content.ReadFromJsonAsync<ReturnBookViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.MemberId.Should().Be(member.Id);
        result.BookCopyId.Should().Be(bookCopyId);
        result.DaysLate.Should().Be(0);
        result.PenaltyAmount.Should().Be(0m);
        result.ReturnedAt.Should().Be(_dateTimeProvider.UtcNow);

        var loan = await fixture.DbContext.Loans
            .AsNoTracking()
            .FirstAsync(l => l.MemberId == member.Id && l.BookCopyId == bookCopyId);

        loan.ReturnedAt.Should().NotBeNull();

        var copy = await fixture.DbContext.BookCopies.FindAsync(bookCopyId);
        copy!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturnsPenaltyAndDaysLateWhenBookIsReturnedLate()
    {
        // Arrange — borrow then artificially set DueDate to 5 days ago
        const int daysLate = 5;
        const decimal expectedPenalty = daysLate * 0.20m;

        var member = await SeedMemberAsync();
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();

        await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/members/{member.Id}/loans",
            new { bookCopyId });

        var pastDueDate = DateTime.UtcNow.AddDays(-daysLate);
        await fixture.DbContext.Database.ExecuteSqlRawAsync(
            @"UPDATE loans SET ""dueDate"" = {0} WHERE ""memberId"" = {1} AND ""bookCopyId"" = {2} AND ""returnedAt"" IS NULL",
            pastDueDate, member.Id, bookCopyId);

        // Act
        var response = await fixture.HttpClient.PostAsync(
            new Uri($"/api/v1/members/{member.Id}/loans/{bookCopyId}/return", UriKind.Relative),
            content: null);

        var result = await response.Content.ReadFromJsonAsync<ReturnBookViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.DaysLate.Should().Be(daysLate);
        result.PenaltyAmount.Should().Be(expectedPenalty);
    }

    [Fact]
    public async Task ShouldReturnsNotFoundWhenMemberDoesNotExist()
    {
        // Act
        var response = await fixture.HttpClient.PostAsync(
            new Uri("/api/v1/members/99999/loans/1/return", UriKind.Relative),
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnsBadRequestWhenNoActiveLoanExistsForBookCopy()
    {
        // Arrange — member exists but has no loan for this copy
        var member = await SeedMemberAsync();

        // Act
        var response = await fixture.HttpClient.PostAsync(
            new Uri($"/api/v1/members/{member.Id}/loans/99999/return", UriKind.Relative),
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails!.Title.Should().Be(MemberErrors.LoanNotFound.Code);
    }

    [Theory]
    [MemberData(nameof(InvalidRouteDataSetup))]
    public async Task ShouldReturnsBadRequestWhenRouteParametersAreInvalid(int memberId, int bookCopyId, ErrorResult expectedError)
    {
        // Act
        var response = await fixture.HttpClient.PostAsync(
            new Uri($"/api/v1/members/{memberId}/loans/{bookCopyId}/return", UriKind.Relative),
            content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problemDetails!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([expectedError]);
    }

    public static TheoryData<int, int, ErrorResult> InvalidRouteDataSetup()
    {
        return new TheoryData<int, int, ErrorResult>
        {
            { 0, 1, MemberErrors.InvalidMemberId },
            { 1, 0, BookErrors.InvalidBookCopyId }
        };
    }

    private async Task<Member> SeedMemberAsync(MembershipType? membershipType = null)
    {
        var member = Member.Create(
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            $"{Guid.NewGuid()}@test.com",
            membershipType ?? MembershipType.Standard,
            DateTime.UtcNow);

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

        var copy = await fixture.DbContext.BookCopies
            .AsNoTracking()
            .FirstAsync(bc => bc.BookId == book.Id);

        return (book, copy.Id);
    }
}
