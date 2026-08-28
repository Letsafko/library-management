using System.Threading;
using Api.Extensions;
using Api.ViewModels;
using Application.Features.Books.Borrow;
using Application.Features.Models;
using Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints.Books.Borrow;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/members/{memberId}/loans", async (
                int memberId,
                Request request,
                IRequestHandler<BorrowBookCommand, LoanResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(memberId);
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.Match(
                    onSuccess: x => Results.Created($"/members/{x.MemberId}/loans/{x.LoanId}", new LoanViewModel(x)),
                    onFailure: CustomResults.Problem);
            })
            .WithName("BorrowBook")
            .WithTags(Tags.Loans)
            .WithSummary("Borrow a book copy")
            .WithDescription("Allows a member to borrow an available book copy.")
            .Produces<LoanViewModel>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
