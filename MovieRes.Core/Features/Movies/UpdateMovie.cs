using FluentValidation;
using MovieRes.Core.Data;
using MovieRes.Core.EndPoints;
using MovieRes.Core.Features.Movies.Requests;
using MovieRes.Core.Features.Movies.Responses;

namespace MovieRes.Core.Features.Movies;

public class UpdateMovie
{
    
    
    public sealed class Validator : BaseMovieRequestValidator<Request>
    {
        public Validator()
        {
        }
    }
    
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/movies/{id}", Handler).WithTags("Movies");
        }
    }

    public async static Task<IResult> Handler(int movieId, Request request, AppDbContext context, IValidator<Request> validator, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request,cancellationToken);

        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        
        if (movieId <= 0) return Results.NotFound();
        
        var updateMovie = await context.Movies.FindAsync([movieId], cancellationToken);

        if (updateMovie is null) return Results.NotFound();
        
        updateMovie.Title = request.Title;
        updateMovie.OriginalTitle = request.OriginalTitle;
        updateMovie.Director = request.Director;
        updateMovie.ReleaseYear = request.ReleaseYear;
        updateMovie.Description = request.Description;
        updateMovie.PosterUrl = request.PosterUrl;
        updateMovie.RuntimeMinutes = request.RuntimeMinutes;

        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(new MovieSummaryResponse(updateMovie.Id, updateMovie.Title, updateMovie.ReleaseYear, updateMovie.PosterUrl));
    }
}