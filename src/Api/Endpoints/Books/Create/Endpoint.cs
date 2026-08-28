using System.Threading;
using Api.Extensions;
using Api.ViewModels;
using Application.Features.Books.Create;
using Application.Features.Models;
using Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints.Books.Create;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/books", async (
                Request request,
                IRequestHandler<CreateBookCommand, BookResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand();
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.Match(
                    onSuccess: x => Results.Created($"/books/{x.BookId}", new BookViewModel(x)),
                    onFailure: CustomResults.Problem);
            })
            .WithName("CreateBook")
            .WithTags(Tags.Books)
            .WithSummary("Create a new book")
            .WithDescription("Creates a new book with a default copy.")
            .Produces<BookViewModel>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
