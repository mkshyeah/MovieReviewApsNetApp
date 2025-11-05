using System.Security.Claims;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Movies;
using MovieRev.Core.Services;

namespace MovieRev.Core.Features.MovieProposals;

public class ApproveMovieProposal
{
    public sealed class EndPoint : IEndPoint
    {
        public void MapEndPoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/proposals/movies/{id}/approve", Handler)
                .WithTags("MovieProposals")
                .RequireAuthorization("ModeratorOrAdmin");
        }
    }

    public static async Task<IResult> Handler(
        int id,
        AppDbContext context,
        TMDbService tmdbService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var proposal = await context.MovieProposals
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (proposal is null) return Results.NotFound("Заявка не найдена.");
        if (proposal.Status != ProposalStatus.Pending) return Results.Conflict("Заявка уже была обработана.");

        var moderatorId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (moderatorId is null) return Results.Unauthorized();

        // Если в заявке есть TMDbId, используем логику импорта
        if (proposal.TMDbId.HasValue)
        {
            // Проверяем, не импортирован ли фильм уже
            var existingMovie = await context.Movies.FirstOrDefaultAsync(
                m => m.TMDbId == proposal.TMDbId.Value, cancellationToken);
            if (existingMovie is not null)
            {
                // Если фильм уже есть, просто одобряем заявку и выходим
                UpdateProposalStatus(proposal, ProposalStatus.Approved, moderatorId);
                await context.SaveChangesAsync(cancellationToken);
                return Results.Ok(new { Message = "Фильм с таким TMDbId уже существует в базе. Заявка одобрена.", MovieId = existingMovie.Id });
            }
            
            // Вызываем хендлер импорта. 
            // Обратите внимание: это вызов метода, а не HTTP-запрос.
            var importResult = await TMDbSearchAndImport.ImportMovie(
                proposal.TMDbId.Value, context, tmdbService, cancellationToken, user);
            
            if (importResult is not IValueHttpResult<Movies.Responses.MovieIdResponse> createdResult)
            {
                // Если импорт не удался, возвращаем его результат
                return importResult;
            }
            
            // Если все успешно, обновляем статус заявки
            UpdateProposalStatus(proposal, ProposalStatus.Approved, moderatorId);
            await context.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { Message = "Заявка одобрена, фильм успешно импортирован.", MovieId = createdResult.Value?.Id });
        }

        // TODO: Добавить логику создания фильма вручную, если TMDbId не указан

        // Пока что, если нет TMDbId, мы не можем автоматически создать фильм.
        // Обновляем статус и сообщаем об этом.
        UpdateProposalStatus(proposal, ProposalStatus.Approved, moderatorId);
        await context.SaveChangesAsync(cancellationToken);
        
        return Results.Ok("Заявка одобрена, но фильм нужно создать вручную, так как TMDbId не был указан.");
    }
    private static void UpdateProposalStatus(MovieProposal proposal, ProposalStatus status, string moderatorId, string? reason = null)
    {
        proposal.Status = status;
        proposal.ReviewedByUserId = moderatorId;
        proposal.ReviewedAt = DateTimeOffset.UtcNow;
        proposal.RejectionReason = reason;
    }
}