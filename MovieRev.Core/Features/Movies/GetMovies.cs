using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.EndPoints;
using MovieRev.Core.Features.Movies.Responses;

namespace MovieRev.Core.Features.Movies;

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
