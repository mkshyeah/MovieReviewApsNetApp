namespace MovieRev.Core.Features.Reviews.Responses;

public record ReviewSummaryResponse(int Id, decimal Rating, string Text, bool IsSpoiler) : IReviewResponse;
