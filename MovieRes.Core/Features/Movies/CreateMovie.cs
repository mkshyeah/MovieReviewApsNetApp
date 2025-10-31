using FluentValidation;
using Microsoft.AspNetCore.Routing; // Используется в IEndpointRouteBuilder
using MovieRes.Core.Data;
using MovieRes.Core.EndPoints;
using System; // Добавлен для DateTime
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieRes.Core.Features.Movies.Requests;
using MovieRes.Core.Features.Movies.Responses;

namespace MovieRes.Core.Features.Movies;

public static class CreateMovie
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/movies", Handler).WithTags("Movies");
        }
    }
    
    public sealed class Validator : AbstractValidator<CreateMovieRequest>
    {
        public Validator(AppDbContext context)
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.OriginalTitle).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Director).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ReleaseYear).InclusiveBetween(1800, DateTime.Now.Year + 5);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.RuntimeMinutes).GreaterThan(0);
            // Валидация, что все переданные ID жанров существуют
            RuleFor(x => x.GenreIds)
                .NotEmpty()
                .MustAsync(async (ids, token) =>
                {
                    var existingCount = await context.Genres.CountAsync(g => ids.Contains(g.Id), token);
                    return existingCount == ids.Count;
                })
                .WithMessage("One or more Genre IDs provided do not exist.");
        }
    }
    
    

    public async static Task<IResult> Handler(CreateMovieRequest request, AppDbContext context, IValidator<CreateMovieRequest> validator, CancellationToken cancellationToken)
    {
        // Валидация введеных данных
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());
        
        // 1. Создание сущности Movie
        var movie = new Movie
        {
            Title = request.Title,
            OriginalTitle = request.OriginalTitle,
            Director = request.Director,
            ReleaseYear = request.ReleaseYear,
            Description = request.Description,
            PosterUrl = request.PosterUrl,
            RuntimeMinutes = request.RuntimeMinutes
        };
        
        // 2. Добавление связей "многие ко многим" (Genre)
        foreach (var genreId in request.GenreIds.Distinct())
        {
            movie.MovieGenres.Add(new MovieGenre { GenreId = genreId });
        }
        
        // 3. Добавление связей "многие ко многим" (Actor - если ActorIds не пусто)
        if (request.ActorIds is not null)
        {
            foreach (var actorId in request.ActorIds.Distinct())
            {
                // NOTE: Здесь мы не знаем Role, поэтому оставляем ее пустой
                movie.MovieActors.Add(new MovieActor { ActorId = actorId, Role = "TBD" });
            }
        }
        
        context.Movies.Add(movie);
        
        await context.SaveChangesAsync(cancellationToken);
        return Results.CreatedAtRoute("GetMovieById", new { id = movie.Id }, new MovieIdResponse(movie.Id));
    }
}
