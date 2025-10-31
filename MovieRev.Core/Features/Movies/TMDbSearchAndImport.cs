using MovieRev.Core.Data;
using MovieRev.Core.EndPoints;
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
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/movies/tmdb/search", SearchMovies).WithTags("Movies").WithName("TMDbSearchMovies");
            app.MapPost("/movies/tmdb/import/{tmdbMovieId}", ImportMovie).WithTags("Movies").WithName("TMDbImportMovie");
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
            "https://image.tmdb.org/t/p/w500" + m.PosterPath ?? string.Empty // Формируем полный URL постера
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
            m => m.OriginalTitle == tmdbMovieId.ToString(), // TODO: Нужно более надежное сравнение, возможно, через новое поле TMDbId
            cancellationToken);

        if (existingMovie is not null)
        {
            return Results.Conflict("Этот фильм уже импортирован.");
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
                    existingActor = new Actor { FullName = tmdbCastMember.Name ?? "Unknown", PhotoUrl = "https://image.tmdb.org/t/p/w500" + tmdbCastMember.ProfilePath };
                    context.Actors.Add(existingActor);
                }
                actors.Add(existingActor);
            }
        }
        await context.SaveChangesAsync(cancellationToken); // Сохраняем новые жанры и актеров, чтобы получить их ID

        // Создаем новый фильм
        var movie = new Movie
        {
            Title = tmdbMovieDetail.Title ?? tmdbMovieDetail.OriginalTitle ?? "",
            OriginalTitle = tmdbMovieDetail.OriginalTitle ?? "",
            Director = tmdbMovieDetail.Credits?.Crew?.FirstOrDefault(c => c.Job == "Director")?.Name ?? "",
            ReleaseYear = (tmdbMovieDetail.ReleaseDate != null && int.TryParse(tmdbMovieDetail.ReleaseDate.Split('-')[0], out var year)) ? year : 0,
            Description = tmdbMovieDetail.Overview ?? "",
            PosterUrl = "https://image.tmdb.org/t/p/w500" + tmdbMovieDetail.PosterPath ?? string.Empty,
            RuntimeMinutes = tmdbMovieDetail.Runtime,
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
}
