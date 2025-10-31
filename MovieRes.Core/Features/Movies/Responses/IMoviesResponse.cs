namespace MovieRes.Core.Features.Movies.Responses;

public interface IMovieResponse
{
    int Id { get; }
    string Title { get; }
    int ReleaseYear { get; }
    string PosterUrl { get; } // Полезно для списка
}