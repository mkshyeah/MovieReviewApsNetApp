namespace MovieRev.Core.Features.Reviews.Responses;

public interface IReviewResponse
{
    int Id { get; }
    decimal Rating { get; }
    string Text { get; }
    bool IsSpoiler { get; }
}