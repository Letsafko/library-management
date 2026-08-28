using System.Threading;
using Api.Extensions;
using Api.ViewModels;
using Application.Features.Books.AddCopy;
using Application.Features.Models;
using Application.Messaging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Endpoints.Books.AddCopy;

internal sealed class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/books/{bookId}/copies", async (
                int bookId,
                Request request,
                IRequestHandler<AddBookCopyCommand, BookResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var command = request.ToCommand(bookId);
                var result = await handler.HandleAsync(command, cancellationToken);

                return result.Match(
                    onSuccess: x => Results.Created($"/books/{x.BookId}/copies", new BookViewModel(x)),
                    onFailure: CustomResults.Problem);
            })
            .WithName("AddBookCopy")
            .WithTags(Tags.Books)
            .WithSummary("Add a copy to an existing book")
            .WithDescription("Adds a new copy to an existing book identified by its id.")
            .Produces<BookViewModel>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
