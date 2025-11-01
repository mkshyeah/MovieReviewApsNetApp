namespace MovieRev.Core.Features.Movies.Responses;

public record MovieDetailedResponse(
    int Id, 
    string Title, 
    int ReleaseYear, 
    string PosterUrl,
    // Дополнительные поля для детализации
    string OriginalTitle, 
    string Director, 
    string Description, 
    int RuntimeMinutes,
    int? TMDbId, // Добавляем TMDbId в ответ
    decimal AverageRating,
    int ReviewCount,
    
    // Поля с таблиц Many-Many
    IReadOnlyList<GenreResponse> Genres,
    IReadOnlyList<ActorResponse> Actors
) : IMovieResponse;