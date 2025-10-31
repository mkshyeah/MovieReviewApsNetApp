using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.EndPoints;
using MovieRev.Core.Features.Reviews.Requests;
using MovieRev.Core.Features.Reviews.Responses;
using System.Security.Claims;

namespace MovieRev.Core.Features.Reviews;

public class UpdateReview
{
    public sealed class RequestValidator(AppDbContext context)
        : BaseReviewRequestValidator<UpdateReviewRequest>(context);
    
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/reviews/{id}", Handler).WithTags("Reviews");
        }
    }

    public async static Task<IResult> Handler(
        int reviewId,
        UpdateReviewRequest request,
        AppDbContext context,
        IValidator<UpdateReviewRequest> validator,
        CancellationToken cancellationToken,
        ClaimsPrincipal user)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        if (reviewId <= 0) return Results.NotFound();

        var updateReview = await context.Reviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);

        if (updateReview is null) return Results.NotFound();
        
        var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId is null || updateReview.UserId != currentUserId)
        {
            return Results.Forbid();
        }
        
        updateReview.Rating = request.Rating;
        updateReview.Text = request.Text;
        updateReview.IsSpoiler = request.IsSpoiler;

        await context.SaveChangesAsync(cancellationToken);
        
        return Results.Ok(new ReviewSummaryResponse(updateReview.Id, updateReview.Rating, updateReview.Text, updateReview.IsSpoiler));
    }
}