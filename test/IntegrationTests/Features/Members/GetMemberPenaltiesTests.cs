using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.ViewModels;
using Bogus;
using Domain.Members;
using Domain.Members.ValueObjects;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Primitives;
using Xunit;

namespace IntegrationTests.Features.Members;

[Collection(ApplicationCollectionFixture.Name)]
public sealed class GetMemberPenaltiesTests(ApplicationFixture fixture)
{
    private readonly Faker _faker = new();
    private readonly IDateTimeProvider _dateTimeProvider
        = fixture.Factory.Services.GetRequiredService<IDateTimeProvider>();

    [Fact]
    public async Task ShouldReturnsZeroWhenMemberHasNoOverdueLoans()
    {
        // Arrange
        var member = await SeedMemberAsync();

        // Act
        var response = await fixture.HttpClient.GetAsync(
            new Uri($"/api/v1/members/{member.Id}/penalties", UriKind.Relative));

        var result = await response.Content.ReadFromJsonAsync<MemberPenaltiesViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.MemberId.Should().Be(member.Id);
        result.TotalPendingPenalty.Should().Be(0m);
    }

    [Fact]
    public async Task ShouldReturnsPendingPenaltyForOverdueLoan()
    {
        // Arrange
        const int daysLate = 5;
        const decimal expectedPenalty = daysLate * 0.20m;

        var member = await SeedMemberAsync();
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();

        await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/members/{member.Id}/loans",
            new { bookCopyId });

        var pastDueDate = _dateTimeProvider.UtcNow.AddDays(-daysLate);
        await fixture.DbContext.Database.ExecuteSqlRawAsync(
            """UPDATE loans SET "dueDate" = {0} WHERE "memberId" = {1} AND "bookCopyId" = {2} AND "returnedAt" IS NULL""",
            pastDueDate, member.Id, bookCopyId);

        // Act
        var response = await fixture.HttpClient.GetAsync(
            new Uri($"/api/v1/members/{member.Id}/penalties", UriKind.Relative));

        var result = await response.Content.ReadFromJsonAsync<MemberPenaltiesViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.MemberId.Should().Be(member.Id);
        result.TotalPendingPenalty.Should().Be(expectedPenalty);
    }

    [Fact]
    public async Task ShouldExcludesReturnedLoansFromPendingTotal()
    {
        // Arrange
        var member = await SeedMemberAsync();
        var (_, bookCopyId) = await CreateBookWithCopyIdAsync();

        await fixture.HttpClient.PostAsJsonAsync(
            $"/api/v1/members/{member.Id}/loans",
            new { bookCopyId });

        var pastDueDate = _dateTimeProvider.UtcNow.AddDays(-5);
        await fixture.DbContext.Database.ExecuteSqlRawAsync(
            """UPDATE loans SET "dueDate" = {0} WHERE "memberId" = {1} AND "bookCopyId" = {2} AND "returnedAt" IS NULL""",
            pastDueDate, member.Id, bookCopyId);

        await fixture.HttpClient.PostAsync(
            new Uri($"/api/v1/members/{member.Id}/loans/{bookCopyId}/return", UriKind.Relative), content: null);

        // Act
        var response = await fixture.HttpClient.GetAsync(
            new Uri($"/api/v1/members/{member.Id}/penalties", UriKind.Relative));

        var result = await response.Content.ReadFromJsonAsync<MemberPenaltiesViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalPendingPenalty.Should().Be(0m);
    }

    [Fact]
    public async Task ShouldReturnsNotFoundWhenMemberDoesNotExist()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync(
            new Uri("/api/v1/members/99999/penalties", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(MemberErrors.MemberNotFound.Code);
    }

    [Fact]
    public async Task ShouldReturnsBadRequestWhenMemberIdIsInvalid()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync(
            new Uri("/api/v1/members/0/penalties", UriKind.Relative));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problem!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([MemberErrors.InvalidMemberId]);
    }

    private async Task<Member> SeedMemberAsync()
    {
        var member = Member.Create(
            _faker.Name.FirstName(),
            _faker.Name.LastName(),
            $"{Guid.NewGuid()}@test.com",
            MembershipType.Standard,
            DateTime.UtcNow);

        fixture.DbContext.Members.Add(member);
        await fixture.DbContext.SaveChangesAsync();
        fixture.DbContext.ChangeTracker.Clear();
        return member;
    }

    private async Task<(object book, int bookCopyId)> CreateBookWithCopyIdAsync()
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
