using Microsoft.AspNetCore.Identity;
using MovieRev.Core.Data;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Admin.Requests;
using MovieRev.Core.Models;

namespace MovieRev.Core.Features.Admin;

public class AssignRoleEndPoint : IEndPoint
{
    public void MapEndPoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/users/{userId}/roles", HandleAsync)
            .RequireAuthorization("AdminOnly")
            .WithTags("Admin")
            .WithSummary("Назначить роль пользователю")
            .WithDescription("Доступно только администраторам.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        AssignRoleRequest request,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Results.NotFound("Пользователь не найден.");
        }
        
        var roleExists = await roleManager.RoleExistsAsync(request.RoleName);
        if (!roleExists)
        {
            return Results.BadRequest($"Роль '{request.RoleName}' не существует.");
        }

        if (await userManager.IsInRoleAsync(user, request.RoleName))
        {
            return Results.BadRequest("Пользователь уже имеет эту роль.");
        }
        
        var result = await userManager.AddToRoleAsync(user, request.RoleName);

        if (result.Succeeded)
        {
            return Results.Ok($"Роль '{request.RoleName}' успешно назначена пользователю {user.UserName}.");
        }
        return Results.BadRequest(result.Errors);
    }
}