using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Reviews.Responses;

namespace MovieRev.Core.Features.Reviews;

public class GetReviews
{
    public sealed class EndPoint: IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/reviews/", Handler).WithTags("Reviews");
        }

        public async static Task<IResult> Handler(
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var reviews = await context.Reviews
                .Select(r => new ReviewSummaryResponse(
                    r.Id,
                    r.Rating,
                    r.Text,
                    r.IsSpoiler))
                .ToListAsync(cancellationToken);
            
            return Results.Ok(reviews);
        }
    }
}