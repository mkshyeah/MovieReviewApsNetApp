namespace MovieRev.Core.Features.Reviews.Responses;

public record CommentResponse(
    int Id,
    string Text,
    DateTimeOffset CommentDate,
    string UserId 
);
