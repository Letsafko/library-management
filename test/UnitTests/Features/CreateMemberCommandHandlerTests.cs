using System;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Application.Features.Members.Create;
using Application.Features.Models;
using Bogus;
using Domain.Members;
using Domain.Members.ValueObjects;
using FluentAssertions;
using Infrastructure.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Primitives;
using Xunit;

namespace UnitTests.Features;

public sealed class CreateMemberCommandFaker : Faker<CreateMemberCommand>
{
    public CreateMemberCommandFaker(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? membershipType = null,
        bool forceNullFirstName = false,
        bool forceNullLastName = false,
        bool forceNullEmail = false,
        bool forceNullMembershipType = false)
    {
        CustomInstantiator(f => new CreateMemberCommand(
            FirstName: forceNullFirstName ? null : firstName ?? f.Name.FirstName(),
            LastName: forceNullLastName ? null : lastName ?? f.Name.LastName(),
            Email: forceNullEmail ? null : email ?? f.Internet.Email(),
            MembershipType: forceNullMembershipType ? null : membershipType ?? MembershipType.Standard.Name));
    }
}

public sealed class CreateMemberCommandHandlerTests : CommandHandlerBaseTests<CreateMemberCommand, MemberResponse>
{
    private readonly LoggingDecorator.RequestHandler<CreateMemberCommand, MemberResponse> _handler;
    private readonly Mock<IMemberRepository> _memberRepository;

    public CreateMemberCommandHandlerTests() : base(new CreateMemberCommandValidator())
    {
        _memberRepository = new Mock<IMemberRepository>();
        var innerHandler = new CreateMemberCommandHandler(_memberRepository.Object, DateTimeProvider.Object);
        var validatorHandler = new ValidationDecorator.RequestHandler<CreateMemberCommand, MemberResponse>(
            innerHandler,
            Services.Object);
        _handler = new LoggingDecorator.RequestHandler<CreateMemberCommand, MemberResponse>(validatorHandler, Logger.Object);
    }

    [Fact]
    public async Task ShouldCreatesAndPersistsMemberWhenRequestIsValid()
    {
        // Arrange
        var command = new CreateMemberCommandFaker(membershipType: "Standard").Generate();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(command.FirstName);
        result.Value.LastName.Should().Be(command.LastName);
        result.Value.Email.Should().Be(command.Email);
        result.Value.MembershipType.Should().Be(MembershipType.Standard);

        _memberRepository.Verify(
            x => x.AddAsync(
                It.Is<Member>(m =>
                    m.FirstName == command.FirstName &&
                    m.LastName == command.LastName &&
                    m.Email == command.Email &&
                    m.MembershipType == MembershipType.Standard &&
                    m.CreatedDatetime == DateTime),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Logger.Verify(x
            => x.Log(LogLevel.Information, $"Processing request {nameof(CreateMemberCommand)}", It.IsAny<Exception?>()), Times.Once);

        Logger.Verify(x
            => x.Log(LogLevel.Information, $"Completed request {nameof(CreateMemberCommand)} successfully.", It.IsAny<Exception?>()), Times.Once);
    }

    [Fact]
    public async Task ShouldCreatesStudentMemberWhenMembershipTypeIsStudent()
    {
        // Arrange
        var command = new CreateMemberCommandFaker(membershipType: "Student").Generate();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.MembershipType.Should().Be(MembershipType.Student);

        _memberRepository.Verify(
            x => x.AddAsync(
                It.Is<Member>(m => m.MembershipType == MembershipType.Student),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldReturnsInvalidMembershipTypeWhenTypeIsUnknown()
    {
        // Arrange
        var command = new CreateMemberCommandFaker(membershipType: "Premium").Generate();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(MemberErrors.InvalidMembershipType));
        _memberRepository.Verify(x => x.AddAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(CreateMemberCommandDataSetup))]
    public async Task ShouldReturnsMissingFieldErrorWhenCommandIsInvalid(CreateMemberCommand command, ErrorResult error)
    {
        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(new ValidationError(error));
        _memberRepository.Verify(x => x.AddAsync(It.IsAny<Member>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static TheoryData<CreateMemberCommand, ErrorResult> CreateMemberCommandDataSetup()
    {
        return new TheoryData<CreateMemberCommand, ErrorResult>
        {
            { new CreateMemberCommandFaker(forceNullFirstName: true).Generate(), MemberErrors.MissingFirstName },
            { new CreateMemberCommandFaker(forceNullLastName: true).Generate(), MemberErrors.MissingLastName },
            { new CreateMemberCommandFaker(forceNullEmail: true).Generate(), MemberErrors.MissingEmail },
            { new CreateMemberCommandFaker(forceNullMembershipType: true).Generate(), MemberErrors.MissingMembershipType }
        };
    }
}