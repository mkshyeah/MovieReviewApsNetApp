using FluentValidation;
using Microsoft.AspNetCore.Identity;
using MovieRev.Core.Extensions;
using MovieRev.Core.Features.Auth.Requests;
using MovieRev.Core.Features.Auth.Response;
using MovieRev.Core.Models;
using MovieRev.Core.Services.Auth;

namespace MovieRev.Core.Features.Auth;

public sealed class AuthEndPoints : IEndPoint
{
    public void MapEndPoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", Register)
            .WithTags("Auth")
            .WithSummary("Регистрация нового пользователя")
            .WithDescription("Регистрирует нового пользователя в системе")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
        
        app.MapPost("/auth/login", Login)
            .WithTags("Auth")
            .WithSummary("Вход пользователя")
            .WithDescription("Позволяет существующему пользователю войти в систему и получить JWT-токен")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }
    
    // Обработчик для регистрации нового пользователя
    public async static Task<IResult> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        JwtTokenService tokenService,
        IValidator<RegisterRequest> validator)
    {
        // 1. Валидация запроса
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        // 2. Создание нового пользователя
        var user = new ApplicationUser()
        {
            UserName = request.UserName,
            Email = request.Email,
            EmailConfirmed = true // Или false, если нужна верификация по email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(result.Errors.Select(e => e.Description));
        }
        
        // Назначения роли
        await userManager.AddToRoleAsync(user, Roles.User);
        
        // 3. Генерация JWT-токена
        var token = await tokenService.GenerateToken(user);
        var expiration = DateTimeOffset.UtcNow.AddDays(7); // Срок действия токена, как в JwtSettings
        
        // 4. Возврат ответа
        return Results.Ok(new AuthResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = token,
            Expiration = expiration
        });
    }
    
    
    // Обработчик для входа пользователя
    public async static Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService tokenService,
        IValidator<LoginRequest> validator)
    {
        // 1. Валидация запроса
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        // 2. Поиск пользователя по имени пользователя или email
        var user = await userManager.FindByNameAsync(request.UserNameOrEmail);
        if (user == null)
        {
            user = await userManager.FindByEmailAsync(request.UserNameOrEmail);
        }
        if (user == null)
        {
            return Results.Unauthorized(); // Пользователь не найден
        }
        
        // 3. Проверка пароля
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false); // false = не блокировать при неудачных попытках

        if (!result.Succeeded)
        {
            return Results.Unauthorized(); // Неверный пароль
        }
        
        // 4. Генерация JWT-токена
        var token = await tokenService.GenerateToken(user);
        var expiration = DateTimeOffset.UtcNow.AddDays(7);
        
        // 5. Возврат ответа
        return Results.Ok(new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Token = token,
                Expiration = expiration
            }
        );
    }
}