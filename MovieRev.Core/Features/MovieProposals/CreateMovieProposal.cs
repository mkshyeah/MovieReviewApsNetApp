using System.Security.Claims;
using FluentValidation;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.MovieProposals.Requests;

namespace MovieRev.Core.Features.MovieProposals;

public class CreateMovieProposal
{
    public sealed class Validator : AbstractValidator<CreateProposalRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
    
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/proposal/movies", Handler)
                .WithTags("Proposal")
                .RequireAuthorization();
        }
    }

    public async static Task<IResult> Handler(
        CreateProposalRequest request,
        AppDbContext context,
        IValidator<CreateProposalRequest> validator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var proposal = new MovieProposal
        {
            Title = request.Title,
            TMDbId = request.TMDbId,
            Notes = request.Notes,
            ProposedByUserId = userId,
        };

        context.MovieProposals.Add(proposal);
        await context.SaveChangesAsync(cancellationToken);
        
        return Results.Accepted();
    }
}