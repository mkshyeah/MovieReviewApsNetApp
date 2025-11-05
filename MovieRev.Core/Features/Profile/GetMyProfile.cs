using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Profile.Responses;
using MovieRev.Core.Models;

namespace MovieRev.Core.Features.Profile;

public class GetMyProfile : IEndPoint
{
    public void MapEndPoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/me", HandleAsync)
            .RequireAuthorization()
            .WithTags("Profile")
            .WithSummary("Получить профиль текущего пользователя")
            .Produces<UserProfileResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal claimsPrincipal,
        AppDbContext context,
        UserManager<ApplicationUser> userManager, // UserManager все еще нужен для получения ролей
        CancellationToken cancellationToken)
    {
        // ИЗМЕНЕНИЕ 1: Правильно получаем ID пользователя
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            // Этого не должно произойти, если эндпоинт защищен авторизацией, но это хорошая проверка
            return Results.Unauthorized();
        }

        var user = await context.Users
            .AsNoTracking() // Используем AsNoTracking для запросов только на чтение
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Movie)
            .Include(u => u.MovieProposals)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Results.NotFound("Пользователь не найден.");
        }

        // ИЗМЕНЕНИЕ 2: Добавлена логика формирования ответа
        var roles = await userManager.GetRolesAsync(user);

        var response = new UserProfileResponse
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            Roles = roles,
            Reviews = user.Reviews.Select(r => new UserReviewSummary(
                r.Id,
                r.Movie?.Title ?? "N/A",
                r.Rating,
                r.Text,
                r.ReviewDate
            )).ToList(),
            Proposals = user.MovieProposals.Select(p => new UserProposalSummary(
                p.Id,
                p.Title,
                p.Status.ToString(),
                p.CreatedAt
            )).OrderByDescending(p => p.CreatedAt).ToList() // Сортируем для удобства
        };

        return Results.Ok(response);
    }
}