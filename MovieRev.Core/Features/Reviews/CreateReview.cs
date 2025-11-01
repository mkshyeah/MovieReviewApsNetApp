using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Reviews.Requests;
using System.Security.Claims;

namespace MovieRev.Core.Features.Reviews;

public class CreateReview
{
    public sealed class RequestValidator : BaseReviewRequestValidator<CreateReviewRequest>
    {
        private readonly AppDbContext _context;

        public RequestValidator(AppDbContext context) : base(context)
        {
            _context = context;

            RuleFor(x => x.MovieId)
                .GreaterThan(0).WithMessage("Недействительный ID фильма.")
                .MustAsync(BeAnExistingMovie).WithMessage("Фильм с указанным ID не существует.");
        }

        private async Task<bool> BeAnExistingMovie(int movieId, CancellationToken cancellationToken)
        {
            return await _context.Movies.AnyAsync(m => m.Id == movieId, cancellationToken);
        }
    }

    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/reviews", Handler).WithTags("Reviews");
        }
    }

    public async static Task<IResult> Handler(
        CreateReviewRequest request, 
        AppDbContext context, 
        IValidator<CreateReviewRequest> validator, 
        CancellationToken cancellationToken,
        ClaimsPrincipal user)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Results.Unauthorized();

        var review = new Review
        {
            Rating = request.Rating,
            Text = request.Text,
            IsSpoiler = request.IsSpoiler,
            MovieId = request.MovieId,
            UserId = userId,
            ReviewDate = DateTimeOffset.UtcNow,
            LikesCount = 0
        };

        context.Reviews.Add(review);
        await context.SaveChangesAsync(cancellationToken);

        return Results.CreatedAtRoute("GetReviewById", new { id = review.Id }, new { Id = review.Id });
    }
}