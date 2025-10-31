using Microsoft.EntityFrameworkCore;
using MovieRes.Core.Data;
using MovieRes.Core.EndPoints;
using MovieRes.Core.Features.Movies.Responses;

namespace MovieRes.Core.Features.Movies;

public class GetMovies
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/movies/", Handler).WithName("Movies");
        }
    }

    public async static Task<IResult> Handler(AppDbContext db, CancellationToken cancellationToken)
    {
        var moviesResponse = await db.Movies.AsNoTracking()
            .Select(m => new MovieSummaryResponse(
                m.Id,
                m.Title,
                m.ReleaseYear,
                m.PosterUrl))
            .ToListAsync(cancellationToken);
            
        return Results.Ok(moviesResponse);
    }
}
