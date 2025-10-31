using MovieRes.Core.Data;
using MovieRes.Core.EndPoints;

namespace MovieRes.Core.Features.Movies;

public class DeleteMovie
{
    public sealed class EndPoint:IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/movies/{id}", Handler).WithTags("Movies");
        }
    }

    public async static Task<IResult> Handler(int movieId, AppDbContext db, CancellationToken cancellationToken)
    {
        if (movieId <= 0) return Results.NotFound();

        
        var deleteMovie = await db.Movies.FindAsync([movieId], cancellationToken);  
        
        if (deleteMovie is null) return Results.NotFound();
        
        db.Movies.Remove(deleteMovie);
        
        await db.SaveChangesAsync(cancellationToken); 
        
        return Results.NoContent();
    }
}