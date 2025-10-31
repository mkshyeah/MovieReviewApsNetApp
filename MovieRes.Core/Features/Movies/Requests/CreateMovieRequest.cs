using System.ComponentModel.DataAnnotations;

namespace MovieRes.Core.Features.Movies.Requests;

public record CreateMovieRequest(
    [Required] string Title, 
    [Required] string OriginalTitle,
    [Required] string Director, 
    [Range(1800, 3000)] int ReleaseYear, 
    [Required] string Description, 
    string PosterUrl, 
    [Range(1, 1000)] int RuntimeMinutes,
    
    // НОВЫЕ ПОЛЯ
    [Required] IReadOnlyList<int> GenreIds,
    IReadOnlyList<int> ActorIds // Опционально
);