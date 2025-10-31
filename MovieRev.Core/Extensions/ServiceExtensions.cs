using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieRev.Core.Data;
using MovieRev.Core.Settings;
using MovieRev.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MovieRev.Core.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Настройка контекста БД
        services.AddDbContext<AppDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Настройка ASP.NET Identity
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Добавляем сервисы Аутентификации и Авторизации.
        services.AddAuthentication();
        services.AddAuthorization();

        // Добавление Strongly-typed настроек TMDb
        services.Configure<TMDbSettings>(configuration.GetSection("TMDbSettings"));

        // Добавление HttpClient и регистрация TMDbService
        services.AddHttpClient<TMDbService>();

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }
}
