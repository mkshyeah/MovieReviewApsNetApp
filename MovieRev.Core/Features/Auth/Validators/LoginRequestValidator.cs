using FluentValidation;
using MovieRev.Core.Features.Auth.Requests;

namespace MovieRev.Core.Features.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty().WithMessage("Имя пользователя или Email являются обязательными.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль является обязательным.");

    }
}
