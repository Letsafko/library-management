using System.Threading;
using Api.Extensions;
using Api.ViewModels;
using Application.Features.Members.GetPenalties;
using Application.Features.Models;
using Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints.Members.GetPenalties;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/members/{memberId}/penalties", async (
                int memberId,
                IRequestHandler<GetMemberPenaltiesQuery, MemberPenaltiesResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new GetMemberPenaltiesQuery(memberId);
                var result = await handler.HandleAsync(query, cancellationToken);

                return result.Match(
                    onSuccess: x => Results.Ok(new MemberPenaltiesViewModel(x)),
                    onFailure: CustomResults.Problem);
            })
            .WithName("GetMemberPenalties")
            .WithTags(Tags.Members)
            .WithSummary("Get total pending penalties for a member")
            .WithDescription("Returns the sum of accruing penalties for all non-returned overdue loans of a member.")
            .Produces<MemberPenaltiesViewModel>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
