using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Reviews.Responses;
using System.Linq;

namespace MovieRev.Core.Features.Reviews;

public class GetReviewById
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/reviews/{id}", Handler)
               .WithTags("Reviews")
               .WithName("GetReviewById");
        }
    }

    public async static Task<IResult> Handler(
        int reviewId,
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        if (reviewId <= 0) return Results.NotFound();
        
        var reviewResponse = await context.Reviews
            .AsNoTracking()
            .Include(r => r.Comments) // Включаем комментарии
            .Include(r => r.Likes)    // Включаем лайки
            .Where(r => r.Id == reviewId)
            .Select(r => new ReviewDetailedResponse( // Исправлено имя класса
                r.Id,
                r.Rating,
                r.Text,
                r.IsSpoiler,
                r.LikesCount,
                r.UserId,
                r.MovieId,
                // Проекция комментариев
                r.Comments.Select(c => new CommentResponse(
                    c.Id,
                    c.Text,
                    c.CreatedDate,
                    c.UserId
                )).ToList(),
                // Проекция лайков
                r.Likes.Select(l => new ReviewLikeResponse(
                    l.UserId,
                    l.CreatedDate
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return reviewResponse is null ? Results.NotFound() : Results.Ok(reviewResponse);
    }
}
