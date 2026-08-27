using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints;

internal interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}