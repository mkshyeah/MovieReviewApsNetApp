using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.Movies.Requests;

public record UpdateMovieRequest(
    [Required] string Title, 
    [Required] string OriginalTitle, 
    [Required] string Director, 
    [Range(1888, 2100)] int ReleaseYear, 
    [Required] string Description, 
    string PosterUrl, 
    [Range(1, 999)] int RuntimeMinutes, 
    
    // Новые поля для связей "многие ко многим"
    [Required] List<int> GenreIds, 
     List<int> ActorIds
) : IMoviesRequest;