using System.ComponentModel.DataAnnotations;

namespace MovieRes.Core.Features.Movies.Requests;

public record UpdateMovieRequest(
    [Required] string Title, 
    [Required] string OriginalTitle, 
    [Required] string Director, 
    [Range(1888, 2100)] int ReleaseYear, 
    [Required] string Description, 
    [Required] string PosterUrl, 
    [Range(1, 999)] int RuntimeMinutes, 
    
    // Новые поля для связей "многие ко многим"
    [Required] List<int> GenreIds, 
    [Required] List<int> ActorIds
);