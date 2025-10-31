namespace MovieRev.Core.Features.Reviews.Requests;

public interface IReviewRequest
{
    decimal Rating { get; }
    string Text { get; }
    bool IsSpoiler { get; }
}