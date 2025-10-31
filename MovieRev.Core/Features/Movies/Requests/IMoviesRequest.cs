namespace MovieRev.Core.Features.Movies.Requests;

public interface IMoviesRequest
{
    string Title { get; }
    string OriginalTitle { get; }
    string Director { get; }
    int ReleaseYear { get; }
    string Description { get; }
    string PosterUrl { get; }
    int RuntimeMinutes { get; }
    
    // Новые поля для связей "многие ко многим"
    List<int> GenreIds { get; }
    List<int> ActorIds { get; }
}