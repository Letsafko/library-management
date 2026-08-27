using System;
using Application.Messaging;
using FluentValidation;
using Moq;
using SharedKernel.Primitives;

namespace UnitTests.Features;

public abstract class CommandHandlerBaseTests<TRequest, TResponse> where TRequest : IRequest
{
    protected DateTime DateTime { get; } = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    protected Mock<MockLogger<IRequestHandler<TRequest, TResponse>>> Logger { get; }
    protected Mock<IDateTimeProvider> DateTimeProvider { get; }
    protected Mock<IServiceProvider> Services { get; }
    
    protected CommandHandlerBaseTests(IValidator<TRequest> validator)
    {
        DateTimeProvider = new Mock<IDateTimeProvider>();
        DateTimeProvider.SetupGet(x => x.UtcNow).Returns(DateTime);
        Logger = new Mock<MockLogger<IRequestHandler<TRequest, TResponse>>>();
        
        Services = new Mock<IServiceProvider>();
        Services
            .Setup(x => x.GetService(typeof(IValidator<TRequest>)))
            .Returns(validator);
    }
}