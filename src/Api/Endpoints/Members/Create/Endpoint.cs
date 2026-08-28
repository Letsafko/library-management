using System.Threading;
using Api.Extensions;
using Api.ViewModels;
using Application.Features.Members.Create;
using Application.Features.Models;
using Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints.Members.Create;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/members", async (
                Request request,
                IRequestHandler<CreateMemberCommand, MemberResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand();
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.Match(
                    onSuccess: x => Results.Created($"/members/{x.MemberId}", new MemberViewModel(x)),
                    onFailure: CustomResults.Problem);
            })
            .WithName("CreateMember")
            .WithTags(Tags.Members)
            .WithSummary("Create a new member")
            .WithDescription("Creates a new library member with a Standard or Student membership.")
            .Produces<MemberViewModel>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
