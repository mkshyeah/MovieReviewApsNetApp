namespace MovieRev.Core.Features.Reviews.Responses;

public record ReviewDetailedResponse(
    int Id,
    decimal Rating,
    string Text,
    bool IsSpoiler,
    int LikesCount,
    string UserId,
    int MovieId,    
    IReadOnlyList<CommentResponse> Comments,
    IReadOnlyList<ReviewLikeResponse> Likes
    ) : IReviewResponse;
