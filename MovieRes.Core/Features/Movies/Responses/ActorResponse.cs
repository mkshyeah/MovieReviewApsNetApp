namespace MovieRes.Core.Features.Movies.Responses;

public record ActorResponse(
    int Id,
    string FullName,
    string? PhotoUrl
);