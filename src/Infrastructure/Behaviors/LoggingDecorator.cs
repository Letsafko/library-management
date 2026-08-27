using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Application.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel.Primitives;

namespace Infrastructure.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class RequestHandler<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> innerHandler,
        ILogger<IRequestHandler<TRequest, TResponse>> logger)
        : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };
        
        public async Task<Result<TResponse>> HandleAsync(TRequest? request, CancellationToken cancellationToken)
        {
            var requestName = GetNestedTypeName();
            logger.LogInformation("Processing request {RequestName}", requestName);

            var result = await innerHandler.HandleAsync(request, cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Completed request {RequestName} successfully.", requestName);
            }
            else
            {
                logger.LogError("Completed request {RequestName} with error(s): {RawError}",
                    requestName,
                    JsonSerializer.Serialize(result.Error, jsonSerializerOptions));
            }

            return result;
        }

        private static string? GetNestedTypeName()
        {
            return typeof(TRequest?).FullName?.Split('.')[^1].Replace("+", "", StringComparison.OrdinalIgnoreCase);
        }
    }
}