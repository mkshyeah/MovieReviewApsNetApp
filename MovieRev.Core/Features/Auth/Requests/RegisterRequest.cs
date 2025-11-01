using System.ComponentModel.DataAnnotations;

namespace MovieRev.Core.Features.Auth.Requests;

public class RegisterRequest
{
    [Required(ErrorMessage = "Имя пользователя является обязательным.")] 
    [MinLength(3, ErrorMessage ="Имя пользователя должно содержать не менее 3 символов.")]
    [MaxLength(20, ErrorMessage = "Имя пользователя не может превышать 20 символов.")]
    public required string UserName { get; set; }
    
    [Required(ErrorMessage = "Email является обязательным.")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Пароль является обязательным.")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать не менее 6 символов.")]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
    
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    public required string ConfirmPassword { get; set; }
}