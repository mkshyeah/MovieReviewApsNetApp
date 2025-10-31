using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.EndPoints;
using MovieRev.Core.Features.Movies.Responses;

namespace MovieRev.Core.Features.Movies;

public class GetMovieById
{
    public sealed class EndPoint: IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/movies/{id}", Handler).WithTags("Movies").WithName("GetMovieById");
        }
    }

    public async static Task<IResult> Handler(
        int movieId,
         AppDbContext context,
         CancellationToken cancellationToken)
    {
        if (movieId <= 0) return Results.NotFound();
        
        var movieResponse = await context.Movies
            .AsNoTracking()
            .Where(m => m.Id == movieId)
            .Select(m => new MovieDetailedResponse(
                m.Id, 
                m.Title, 
                m.ReleaseYear, 
                m.PosterUrl, 
                m.OriginalTitle, 
                m.Director, 
                m.Description, 
                m.RuntimeMinutes,
                m.AverageRating,
                m.ReviewCount,
                
                // Проекция Жанров через связующую таблицу MovieGenre
                m.MovieGenres
                    .Select(mg => new GenreResponse(mg.GenreId, mg.Genre.Name))
                    .ToList(),
                
                // Проекция Актеров через связующую таблицу MovieActor
                m.MovieActors
                    .Select(ma => new ActorResponse(ma.ActorId, ma.Actor.FullName, ma.Actor.PhotoUrl))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return movieResponse is null ? Results.NotFound() : Results.Ok(movieResponse);
    }
}