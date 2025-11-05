using System.Security.Claims;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Models;

// Добавляем для ClaimsPrincipal

namespace MovieRev.Core.Features.Reviews;

public class DeleteReview
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app) // Changed from MapEndPoint to MapEndPoints
        {
            app.MapDelete("/reviews/{id}", Handler)
                .WithTags("Reviews")
                .RequireAuthorization();
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
        var isOwner = deleteReview.UserId == currentUserId;
        var isModeratorOrAdmin = user.IsInRole(Roles.Moderator) || user.IsInRole(Roles.Administrator);
        
        if (!isOwner && !isModeratorOrAdmin)
        {
            return Results.Forbid(); 
        }

        context.Remove(deleteReview);
        
        await context.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    } 
}