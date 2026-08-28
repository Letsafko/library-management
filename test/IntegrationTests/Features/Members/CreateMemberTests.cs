using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Endpoints.Members.Create;
using Api.ViewModels;
using Bogus;
using Domain.Members;
using FluentAssertions;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Primitives;
using Xunit;

namespace IntegrationTests.Features.Members;

[Collection(ApplicationCollectionFixture.Name)]
public sealed class CreateMemberTests(ApplicationFixture fixture)
{
    [Fact]
    public async Task ShouldCreatesAndPersistsMemberWhenRequestIsValid()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/members", request);
        var result = await response.Content.ReadFromJsonAsync<MemberViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        result!.MemberId.Should().BeGreaterThan(0);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.MembershipType.Should().Be(request.MembershipType);

        var persisted = await fixture.DbContext.Members.FindAsync(result.MemberId);

        persisted!.FirstName.Should().Be(request.FirstName);
        persisted.LastName.Should().Be(request.LastName);
        persisted.Email.Should().Be(request.Email);
        persisted.MembershipType.Name.Should().Be(request.MembershipType);
    }

    [Fact]
    public async Task ShouldCreatesStudentMemberWhenMembershipTypeIsStudent()
    {
        // Arrange
        var request = CreateRequest(membershipType: "Student");

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/members", request);
        var result = await response.Content.ReadFromJsonAsync<MemberViewModel>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result!.MembershipType.Should().Be("Student");

        var persisted = await fixture.DbContext.Members.FindAsync(result.MemberId);
        persisted!.MembershipType.Name.Should().Be("Student");
    }

    [Theory]
    [MemberData(nameof(CreateMemberRequestDataSetup))]
    public async Task ShouldReturnsBadRequestWhenRequestIsInvalid(Request request, ErrorResult expectedError)
    {
        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/members", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problemDetails!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([expectedError]);
    }

    [Fact]
    public async Task ShouldReturnsBadRequestWhenMembershipTypeIsInvalid()
    {
        // Arrange
        var request = CreateRequest(membershipType: "Gold");

        // Act
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/v1/members", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        var errors = ((JsonElement)problemDetails!.Extensions["errors"]!).Deserialize<List<ErrorTest>>();
        errors.Should().BeEquivalentTo([MemberErrors.InvalidMembershipType]);
    }

    public static TheoryData<Request, ErrorResult> CreateMemberRequestDataSetup()
    {
        return new TheoryData<Request, ErrorResult>
        {
            { CreateRequest(forceNullFirstName: true), MemberErrors.MissingFirstName },
            { CreateRequest(forceNullLastName: true), MemberErrors.MissingLastName },
            { CreateRequest(forceNullEmail: true), MemberErrors.MissingEmail },
            { CreateRequest(forceNullMembershipType: true), MemberErrors.MissingMembershipType }
        };
    }

    private static Request CreateRequest(
        string? membershipType = "Standard",
        bool forceNullFirstName = false,
        bool forceNullLastName = false,
        bool forceNullEmail = false,
        bool forceNullMembershipType = false)
    {
        return new Faker<Request>()
            .RuleFor(x => x.FirstName, f => forceNullFirstName ? null : f.Name.FirstName())
            .RuleFor(x => x.LastName, f => forceNullLastName ? null : f.Name.LastName())
            .RuleFor(x => x.Email, f => forceNullEmail ? null : $"{Guid.NewGuid()}@test.com")
            .RuleFor(x => x.MembershipType, _ => forceNullMembershipType ? null : membershipType)
            .Generate();
    }
}
