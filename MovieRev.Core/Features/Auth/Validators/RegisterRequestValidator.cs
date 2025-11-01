using FluentValidation;
using Microsoft.AspNetCore.Identity;
using MovieRev.Core.Data;
using MovieRev.Core.Features.Auth.Requests;
using MovieRev.Core.Models;

namespace MovieRev.Core.Features.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public RegisterRequestValidator(UserManager<ApplicationUser> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя является обязательным.")
            .Length(3, 20).WithMessage("Имя пользователя должно содержать от 3 до 20 символов.")
            .MustAsync(BeUniqueUserName).WithMessage("Имя пользователя уже занято.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email является обязательным.")
            .EmailAddress().WithMessage("Некорректный формат Email.")
            .MustAsync(BeUniqueEmail).WithMessage("Email уже зарегистрирован.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль является обязательным.")
            .MinimumLength(6).WithMessage("Пароль должен содержать не менее 6 символов.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Подтверждение пароля является обязательным.")
            .Equal(x => x.Password).WithMessage("Пароли не совпадают.");
    }

    private async Task<bool> BeUniqueUserName(string userName, CancellationToken cancellationToken)
    {
        return await _userManager.FindByNameAsync(userName) == null;
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return await _userManager.FindByEmailAsync(email) == null;
    }
}