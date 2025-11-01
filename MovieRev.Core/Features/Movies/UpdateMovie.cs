using FluentValidation;
using MovieRev.Core.Features.Movies.Requests;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Movies.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MovieRev.Core.Features.Movies;

public class UpdateMovie
{
    public sealed class Validator : BaseMovieRequestValidator<UpdateMovieRequest>
    {
        public Validator(AppDbContext context) : base(context)
        {
        }
    }
    
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapPut("/movies/{id}", Handler)
                .WithTags("Movies")
                .RequireAuthorization();;
        }
    }

    public async static Task<IResult> Handler(
        int movieId,
        UpdateMovieRequest request,
        AppDbContext context,
        IValidator<UpdateMovieRequest> validator,
        CancellationToken cancellationToken,
        ClaimsPrincipal user) 
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        if (movieId <= 0) return Results.NotFound();
        
        // Загружаем фильм вместе со связанными жанрами и актерами
        var updateMovie = await context.Movies
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == movieId, cancellationToken);

        if (updateMovie is null) return Results.NotFound();
        
        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId is null || updateMovie.UserId != currentUserId)
        {
            return Results.Forbid(); // Только автор может обновлять фильм
        }
        
        // Обновление базовых полей
        updateMovie.Title = request.Title;
        updateMovie.OriginalTitle = request.OriginalTitle;
        updateMovie.Director = request.Director;
        updateMovie.ReleaseYear = request.ReleaseYear;
        updateMovie.Description = request.Description;
        updateMovie.PosterUrl = request.PosterUrl;
        updateMovie.RuntimeMinutes = request.RuntimeMinutes;

        // Обновление жанров: удаляем старые и добавляем новые
        context.Set<MovieGenre>().RemoveRange(updateMovie.MovieGenres);
        updateMovie.MovieGenres = request.GenreIds
            .Distinct()
            .Select(genreId => new MovieGenre { MovieId = movieId, GenreId = genreId })
            .ToList();

        // Обновление актеров: удаляем старых и добавляем новых (если указаны)
        context.Set<MovieActor>().RemoveRange(updateMovie.MovieActors);
        if (request.ActorIds != null && request.ActorIds.Count > 0)
        {
            updateMovie.MovieActors = request.ActorIds
                .Distinct()
                .Select(actorId => new MovieActor { MovieId = movieId, ActorId = actorId, Role = string.Empty })
                .ToList();
        }

        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(new MovieSummaryResponse(updateMovie.Id, updateMovie.Title, updateMovie.ReleaseYear, updateMovie.PosterUrl));
    }
}