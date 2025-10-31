using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Features.Reviews.Requests;

namespace MovieRev.Core.Features.Reviews;

public abstract class BaseReviewValidator<T> : AbstractValidator<T>
    where T : IReviewRequest
{
    protected BaseReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1.0m, 5.0m)
            .WithMessage("Рейтинг должен быть от 1 до 5.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст отзыва не может быть пустым.")
            .MinimumLength(10).WithMessage("Текст отзыва должен содержать не менее 10 символов.")
            .MaximumLength(1000).WithMessage("Текст отзыва не должен превышать 1000 символов.");

        RuleFor(x => x.IsSpoiler)
            .NotNull().WithMessage("Необходимо указать, является ли отзыв спойлером.");
    }
}