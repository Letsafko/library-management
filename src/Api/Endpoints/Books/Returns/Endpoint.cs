using System.Threading;
using Api.Extensions;
using Api.ViewModels;
using Application.Features.Books.Returns;
using Application.Features.Models;
using Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints.Books.Returns;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/members/{memberId}/loans/{bookCopyId}/return", async (
                int memberId,
                int bookCopyId,
                IRequestHandler<ReturnBookCommand, ReturnBookResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = new ReturnBookCommand(memberId, bookCopyId);
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.Match(
                    onSuccess: x => Results.Ok(new ReturnBookViewModel(x)),
                    onFailure: CustomResults.Problem);
            })
            .WithName("ReturnBook")
            .WithTags(Tags.Loans)
            .WithSummary("Return a borrowed book copy")
            .WithDescription("Allows a member to return a borrowed book copy. Returns penalty if overdue.")
            .Produces<ReturnBookViewModel>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
