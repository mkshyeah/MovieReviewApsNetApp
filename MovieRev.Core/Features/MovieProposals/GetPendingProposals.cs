using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.MovieProposals.Responses;

namespace MovieRev.Core.Features.MovieProposals;

public class GetPendingProposals
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/proposals/movies/pending", Handler)
                .WithTags("MovieProposals")
                .RequireAuthorization("ModeratorOrAdmin"); // Только модераторы и админы
        }
    }

    public async static Task<IResult> Handler(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var proposals = await context.MovieProposals
            .AsNoTracking()
            .Include(p => p.ProposedByUser)
            .Where(p => p.Status == ProposalStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProposalSummaryResponse(
                p.Id,
                p.Title,
                p.TMDbId,
                p.Status.ToString(),
                p.ProposedByUser.UserName ?? "N/A",
                p.CreatedAt))
            .ToListAsync(cancellationToken);
        
        return Results.Ok(proposals);
    }
}