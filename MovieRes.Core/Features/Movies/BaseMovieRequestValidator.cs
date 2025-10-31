using FluentValidation;
using MovieRes.Core.Features.Movies.Requests;

namespace MovieRes.Core.Features.Movies;

public abstract class BaseMovieRequestValidator<T> : AbstractValidator<T>
    where T: IMoviesRequest
{
    protected BaseMovieRequestValidator()
    {
        RuleFor(m => m.Title).NotEmpty().WithMessage("Название обязательно");
        RuleFor(m => m.OriginalTitle).NotEmpty().WithMessage("Оригинальное название обязательно");
        RuleFor(m => m.Director).NotEmpty().WithMessage("Режиссер обязателен");
            
        RuleFor(m => m.ReleaseYear)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Year) // Вычисляется при каждом запросе
            .GreaterThanOrEqualTo(1888)
            .WithMessage("'Release Year' должен быть действительным годом (между 1888 и текущим годом).");

        // ПРИМЕЧАНИЕ: Оставлено 500 символов, как было в вашем коде.
        // FluentValidation автоматически подставит фактическое минимальное и максимальное значение
        RuleFor(m => m.Description)
            .MinimumLength(200)
            .WithMessage("Описание должно содержать не менее {MinLength} символов. Вы ввели {TotalLength}.")
            .MaximumLength(500)
            .WithMessage("Описание не должно превышать {MaxLength} символов. Вы ввели {TotalLength}.")
            .NotEmpty().WithMessage("Описание фильма не может быть пустым.");

        RuleFor(m => m.Genre)
            .NotEmpty().WithMessage("Жанр фильма обязателен для заполнения.");

        RuleFor(m => m.RuntimeMinutes)
            .GreaterThan(0).WithMessage("Продолжительность фильма (в минутах) должна быть больше нуля.");
    }
}