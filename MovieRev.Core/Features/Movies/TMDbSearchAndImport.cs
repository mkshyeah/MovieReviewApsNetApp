using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Movies.Responses;
using MovieRev.Core.Models.TMDb;
using MovieRev.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MovieRev.Core.Features.Movies;

public class TMDbSearchAndImport
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/movies/tmdb/search", SearchMovies)
                .WithTags("Movies")
                .WithName("TMDbSearchMovies")
                .AllowAnonymous();
            app.MapPost("/movies/tmdb/import/{tmdbMovieId}", ImportMovie)
                .WithTags("Movies")
                .WithName("TMDbImportMovie")
                .RequireAuthorization("AdminOnly");
        }
    }

    public async static Task<IResult> SearchMovies(
        string query,
        TMDbService tmdbService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.BadRequest("Параметр 'query' не может быть пустым.");
        }

        var searchResult = await tmdbService.SearchMovies(query, cancellationToken);

        if (searchResult is null || !searchResult.Results.Any())
        {
            return Results.NotFound("Фильмы по вашему запросу не найдены на TMDb.");
        }

        // TODO: Возможно, стоит сделать более детальный DTO для результатов поиска
        return Results.Ok(searchResult.Results.Select(m => new MovieSummaryResponse(
            m.Id, // Здесь TMDb Id
            m.Title ?? "Неизвестно",
            (m.ReleaseDate != null && int.TryParse(m.ReleaseDate.Split('-')[0], out var year)) ? year : 0,
            BuildImageUrl(m.PosterPath)
        )));
    }

    public async static Task<IResult> ImportMovie(
        int tmdbMovieId,
        AppDbContext context,
        TMDbService tmdbService,
        CancellationToken cancellationToken,
        ClaimsPrincipal user) // Только авторизованный пользователь может импортировать
    {
        // TODO: Добавить проверку авторизации (например, только администраторы)
        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId is null) return Results.Unauthorized();

        // Проверяем, не импортирован ли фильм уже
        var existingMovie = await context.Movies.FirstOrDefaultAsync(
            m => m.TMDbId == tmdbMovieId, 
            cancellationToken);

        if (existingMovie is not null)
        {
            return Results.Conflict("Этот фильм уже импортирован (TMDbId: " + existingMovie.TMDbId + ").");
        }

        var tmdbMovieDetail = await tmdbService.GetMovieDetails(tmdbMovieId, cancellationToken);

        if (tmdbMovieDetail is null)
        {
            return Results.NotFound("Детали фильма на TMDb не найдены.");
        }

        // Создаем новые сущности Genre, если их нет в нашей базе
        var genres = new List<Genre>();
        if (tmdbMovieDetail.Genres != null)
        {
            foreach (var tmdbGenre in tmdbMovieDetail.Genres)
            {
                var existingGenre = await context.Genres.FirstOrDefaultAsync(g => g.Name == tmdbGenre.Name, cancellationToken);
                if (existingGenre == null)
                {
                    existingGenre = new Genre { Name = tmdbGenre.Name ?? "Unknown" };
                    context.Genres.Add(existingGenre);
                }
                genres.Add(existingGenre);
            }
        }

        // Создаем новые сущности Actor, если их нет в нашей базе
        var actors = new List<Actor>();
        if (tmdbMovieDetail.Credits?.Cast != null)
        {
            foreach (var tmdbCastMember in tmdbMovieDetail.Credits.Cast.OrderBy(c => c.Order).Take(5))
            {
                var existingActor = await context.Actors.FirstOrDefaultAsync(a => a.FullName == tmdbCastMember.Name, cancellationToken);
                if (existingActor == null)
                {
                    var photoUrl = BuildImageUrl(tmdbCastMember.ProfilePath);
                    existingActor = new Actor
                    {
                        FullName = tmdbCastMember.Name ?? "Unknown",
                        PhotoUrl = photoUrl
                    };
                    context.Actors.Add(existingActor);
                }
                else
                {
                    var updatedPhotoUrl = BuildImageUrl(tmdbCastMember.ProfilePath);
                    if (!string.IsNullOrWhiteSpace(updatedPhotoUrl))
                    {
                        existingActor.PhotoUrl = updatedPhotoUrl;
                    }
                    else if (string.IsNullOrWhiteSpace(existingActor.PhotoUrl)
                             || existingActor.PhotoUrl.Equals("https://image.tmdb.org/t/p/w500", StringComparison.OrdinalIgnoreCase))
                    {
                        existingActor.PhotoUrl = string.Empty;
                    }
                }

                actors.Add(existingActor);
            }
        }
        await context.SaveChangesAsync(cancellationToken); // Сохраняем новые жанры и актеров, чтобы получить их ID

        // Создаем новый фильм
        var movie = new Movie
        {
            Title = tmdbMovieDetail.Title ?? tmdbMovieDetail.OriginalTitle ?? string.Empty,
            OriginalTitle = tmdbMovieDetail.OriginalTitle ?? string.Empty,
            Director = tmdbMovieDetail.Credits?.Crew?.FirstOrDefault(c => c.Job == "Director")?.Name ?? string.Empty,
            ReleaseYear = (tmdbMovieDetail.ReleaseDate != null && int.TryParse(tmdbMovieDetail.ReleaseDate.Split('-')[0], out var year)) ? year : 0,
            Description = tmdbMovieDetail.Overview ?? string.Empty,
            PosterUrl = BuildImageUrl(tmdbMovieDetail.PosterPath),
            RuntimeMinutes = tmdbMovieDetail.Runtime,
            TMDbId = tmdbMovieId, // Присваиваем TMDbId
            UserId = currentUserId // Присваиваем ID текущего пользователя
        };


        // Добавляем связи Many-to-Many
        foreach (var genre in genres)
        {
            movie.MovieGenres.Add(new MovieGenre { Genre = genre });
        }
        foreach (var actor in actors)
        {
            var castMember = tmdbMovieDetail.Credits?.Cast?.FirstOrDefault(c => c.Name == actor.FullName);
            movie.MovieActors.Add(new MovieActor { Actor = actor, Role = castMember?.Character ?? string.Empty });
        }

        context.Movies.Add(movie);
        await context.SaveChangesAsync(cancellationToken);

        return Results.CreatedAtRoute("GetMovieById", new { id = movie.Id }, new MovieIdResponse(movie.Id));
    }

    private static string BuildImageUrl(string? path) => string.IsNullOrWhiteSpace(path)
        ? string.Empty
        : $"https://image.tmdb.org/t/p/w500{path}";
}
