using System.Threading;
using System.Threading.Tasks;
using SharedKernel.Primitives;

namespace Application.Messaging;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest
{
    Task<Result<TResponse>> HandleAsync(TRequest? request, CancellationToken cancellationToken);
}