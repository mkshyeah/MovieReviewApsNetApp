using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.Reviews.Requests;

public record CreateReviewRequest(
    [Range(1, 5)] decimal Rating, 
    [Required, StringLength(1000, MinimumLength = 10)] string Text, 
    bool IsSpoiler, 
    [Required] int MovieId
) : IReviewRequest;