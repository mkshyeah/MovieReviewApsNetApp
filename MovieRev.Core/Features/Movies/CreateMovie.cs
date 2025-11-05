using FluentValidation;
using Microsoft.AspNetCore.Routing; // Используется в IEndpointRouteBuilder
using System; // Добавлен для DateTime
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Features.Movies.Requests;
using MovieRev.Core.Features.Movies.Responses;
using System.Security.Claims; // Добавляем для ClaimsPrincipal
using MovieRev.Core.Extensions;

namespace MovieRev.Core.Features.Movies;

public static class CreateMovie
{
    public sealed class Validator : BaseMovieRequestValidator<CreateMovieRequest>
    {
        public Validator(AppDbContext context) : base(context)
        {
            // Базовая валидация определена в BaseMovieRequestValidator
        }
    }
    
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app) // Changed from MapEndPoint to MapEndPoints
        {
            app.MapPost("/movies", Handler)
                .WithTags("Movies")
                .RequireAuthorization("AdminOnly");
        }
    }

    public async static Task<IResult> Handler(
        CreateMovieRequest request, 
        AppDbContext context, 
        IValidator<CreateMovieRequest> validator, 
        CancellationToken cancellationToken,
        ClaimsPrincipal user)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());
        
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Results.Unauthorized();

        // 1. Создание сущности Movie
        var movie = new Movie
        {
            Title = request.Title,
            OriginalTitle = request.OriginalTitle,
            Director = request.Director,
            ReleaseYear = request.ReleaseYear,
            Description = request.Description,
            PosterUrl = request.PosterUrl,
            RuntimeMinutes = request.RuntimeMinutes,
            UserId = userId 
        };
        
        // 2. Добавление связей "многие ко многим" (Genre)
        foreach (var genreId in request.GenreIds.Distinct())
        {
            movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });
        }
        
        // 3. Добавление связей "многие ко многим" (Actor - если ActorIds не пусто)
        if (request.ActorIds is not null && request.ActorIds.Count > 0)
        {
            foreach (var actorId in request.ActorIds.Distinct())
            {
                // NOTE: Здесь мы не знаем Role, поэтому оставляем ее пустой
                movie.MovieActors.Add(new MovieActor { ActorId = actorId, Role = string.Empty });
            }
        }
        
        context.Movies.Add(movie);
        
        await context.SaveChangesAsync(cancellationToken);
        return Results.CreatedAtRoute("GetMovieById", new { id = movie.Id }, new MovieIdResponse(movie.Id));
    }
}
