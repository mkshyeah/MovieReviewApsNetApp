using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Features.Movies.Requests;

namespace MovieRev.Core.Features.Movies;

public abstract class BaseMovieRequestValidator<T> : AbstractValidator<T>
    where T: IMoviesRequest
{
    private readonly AppDbContext _context; 

    protected BaseMovieRequestValidator(AppDbContext context) 
    {
        _context = context; 

        RuleFor(m => m.Title).NotEmpty().WithMessage("Название обязательно");
        RuleFor(m => m.OriginalTitle).NotEmpty().WithMessage("Оригинальное название обязательно");
        RuleFor(m => m.Director).NotEmpty().WithMessage("Режиссер обязателен");
            
        RuleFor(m => m.ReleaseYear)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Year) 
            .GreaterThanOrEqualTo(1888)
            .WithMessage("'Release Year' должен быть действительным годом (между 1888 и текущим годом).");

        RuleFor(m => m.Description)
            .MinimumLength(200)
            .WithMessage("Описание должно содержать не менее {MinLength} символов. Вы ввели {TotalLength}.")
            .MaximumLength(500)
            .WithMessage("Описание не должно превышать {MaxLength} символов. Вы ввели {TotalLength}.")
            .NotEmpty().WithMessage("Описание фильма не может быть пустым.");

        RuleFor(m => m.RuntimeMinutes)
            .GreaterThan(0).WithMessage("Продолжительность фильма (в минутах) должна быть больше нуля.");

        RuleFor(m => m.GenreIds)
            .NotEmpty().WithMessage("Необходимо указать хотя бы один жанр.")
            .MustAsync(async (ids, token) =>
            {
                if (ids == null || ids.Count == 0) return false;
                var existingCount = await _context.Genres.CountAsync(g => ids.Contains(g.Id), token); 
                return existingCount == ids.Count;
            })
            .WithMessage("Один или несколько указанных жанров не существуют в базе данных.");

        RuleFor(m => m.ActorIds)
            .MustAsync(async (ids, token) =>
            {
                if (ids == null || ids.Count == 0) return true; 
                var existingCount = await _context.Actors.CountAsync(a => ids.Contains(a.Id), token); 
                return existingCount == ids.Count;
            })
            .WithMessage("Один или несколько указанных актеров не существуют в базе данных.");
    }
}