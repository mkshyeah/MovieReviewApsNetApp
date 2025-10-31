namespace MovieRes.Core.Features.Movies.Responses;

public record MovieSummaryResponse(int Id, string Title, int ReleaseYear, string PosterUrl) : IMovieResponse;
