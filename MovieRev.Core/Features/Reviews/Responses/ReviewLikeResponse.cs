namespace MovieRev.Core.Features.Reviews.Responses;

public record ReviewLikeResponse(
    string UserId,
    DateTimeOffset LikeDate
);
