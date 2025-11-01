using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.Auth.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Имя пользователя или Email являются обязательными.")]
    public required string UserNameOrEmail { get; set; }
    
    [Required(ErrorMessage = "Пароль является обязательным.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}