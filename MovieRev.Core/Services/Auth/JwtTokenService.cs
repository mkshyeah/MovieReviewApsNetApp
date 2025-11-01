using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MovieRev.Core.Data;
using MovieRev.Core.Models;
using MovieRev.Core.Settings;

namespace MovieRev.Core.Services.Auth;

public class JwtTokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _jwtSettings= jwtSettings.Value;
    }

    public async Task<string> GenerateToken(ApplicationUser user)
    {
        // 1. Создаем список claims (утверждений), которые будут храниться в токене
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject: идентификатор пользователя
            new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()), // JWT ID: уникальный идентификатор токена
            new Claim(JwtRegisteredClaimNames.Email, user.Email!), // Email пользователя
            new Claim(ClaimTypes.NameIdentifier, user.Id), // Идентификатор пользователя для Identity
            new Claim(ClaimTypes.Name, user.UserName!) // Имя пользователя

        };
        
        // Добавляем роли пользователя в claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        // 2. Создаем ключ для подписи токена
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // 3. Создаем дескриптор токена
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpireDays),
            signingCredentials: creds
        );
        
        // 4. Записываем токен как строку и возвращаем его
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}