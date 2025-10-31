namespace MovieRes.Core.Features.Movies.Requests;

public interface IMoviesRequest
{
    string Title { get; }
    string OriginalTitle { get; }
    string Director { get; }
    int ReleaseYear { get; }
    string Description { get; }
    string PosterUrl { get; }
    string Genre { get; }
    int RuntimeMinutes { get; }
}