using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.Reviews.Requests;

public record UpdateReviewRequest(
    [Range(1,5)] decimal Rating ,
    [Required] string Text ,
    bool IsSpoiler
    ) : IReviewRequest;