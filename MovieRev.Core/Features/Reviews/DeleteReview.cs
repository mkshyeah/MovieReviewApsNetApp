using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using System.Security.Claims; // Добавляем для ClaimsPrincipal

namespace MovieRev.Core.Features.Reviews.Responses;

public class DeleteReview
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/reviews/{id}", Handler).WithTags("Reviews");
        }
    }

    public async static Task<IResult> Handler(
        int reviewId,
        AppDbContext context,
        CancellationToken cancellationToken,
        ClaimsPrincipal user)
    {
        if (reviewId <= 0) return Results.NotFound();

        var deleteReview = await context.Reviews.FindAsync([reviewId], cancellationToken);

        if (deleteReview is null) return Results.NotFound();

        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Получаем ID текущего пользователя
        if (currentUserId is null || deleteReview.UserId != currentUserId)
        {
            return Results.Forbid(); // Отказ в доступе, если пользователь не является автором
        }

        context.Remove(deleteReview);
        
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent(); // Изменяем на Results.NoContent()
    } 
}