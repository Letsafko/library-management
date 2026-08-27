using Application.Features.Books.Create;
using Application.Messaging;
using Infrastructure.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class PipelineBehaviorExtensions
{
    internal static void AddPipelineBehaviors(this IServiceCollection services)
    {
        var requestHandlerTypes = new[]
        {
            typeof(CreateBookCommandHandler),
            typeof(LoggingDecorator.RequestHandler<,>),
            typeof(ValidationDecorator.RequestHandler<,>),
        };
        
        services.Scan(scan => scan.FromAssembliesOf(requestHandlerTypes)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator.RequestHandler<,>));
        services.Decorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator.RequestHandler<,>));
    }
}