using System.Security.Claims;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;

namespace MovieRev.Core.Features.MovieProposals;

public class RejectMovieProposal
{
    public sealed class Validator : AbstractValidator<RejectProposalRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        }
    }
    
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/proposals/movies/{id}/reject", Handler)
                .WithTags("MovieProposals")
                .RequireAuthorization("ModeratorOrAdmin");
        }
    }

    public static async Task<IResult> Handler(
        int id,
        RejectProposalRequest request,
        AppDbContext context,
        IValidator<RejectProposalRequest> validator,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var proposal = await context.MovieProposals
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proposal is null) return Results.NotFound("Заявка не найдена.");
        if (proposal.Status != ProposalStatus.Pending) return Results.Conflict("Заявка уже была обработана.");

        var moderatorId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (moderatorId is null) return Results.Unauthorized();

        proposal.Status = ProposalStatus.Rejected;
        proposal.ReviewedByUserId = moderatorId;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;
        proposal.RejectionReason = request.Reason;
        
        await context.SaveChangesAsync(cancellationToken);
        return Results.Ok("Заявка была отклонена.");
    }


}