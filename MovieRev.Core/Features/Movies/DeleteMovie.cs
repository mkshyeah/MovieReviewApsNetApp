using System.Security.Claims;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;

namespace MovieRev.Core.Features.Movies;

public class DeleteMovie
{
    public sealed class EndPoint:IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapDelete("/movies/{id}", Handler)
                .WithTags("Movies")
                .RequireAuthorization("AdminOnly");
        }
    }

    public async static Task<IResult> Handler(
        int movieId,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (movieId <= 0) return Results.NotFound();
        
        var deleteMovie = await db.Movies.FindAsync([movieId], cancellationToken);  
        
        if (deleteMovie is null) return Results.NotFound();
        
        // Проверка, является ли текущий пользователь создателем фильма
        var currentUser = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUser is null || deleteMovie.UserId != currentUser)
        {
            return Results.Forbid(); // Отказ в доступе, если пользователь не является создателем
        }
        
        db.Movies.Remove(deleteMovie);
        
        await db.SaveChangesAsync(cancellationToken); 
        
        return Results.NoContent();
    }
}