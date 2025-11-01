// MovieRev.Core/Extensions/ServiceExtensions.cs

using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieRev.Core.Data;
using MovieRev.Core.Models;
using MovieRev.Core.Services;
using MovieRev.Core.Services.Auth;
using MovieRev.Core.Settings;

namespace MovieRev.Core.Extensions;

public static class ServiceExtensions
{
    // Метод для настройки базы данных
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => 
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Добавление Strongly-typed настроек JWT
        // Эта строка должна быть ДО настройки JwtBearer, чтобы jwtSettings был доступен
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Правильное получение strongly-typed объекта JwtSettings
                var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
                if (jwtSettings == null)
                {
                    throw new InvalidOperationException("JWT settings are missing or invalid in configuration.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });
        
        // Объединяем два вызова AddAuthorization в один
        services.AddAuthorization();
        
        // Регистрация JwtTokenService
        services.AddScoped<JwtTokenService>();
        
        return services;
    }
    
    public static IServiceCollection AddTMDbIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        // Добавление Strongly-typed настроек TMDb
        services.Configure<TMDbSettings>(configuration.GetSection("TMDbSettings"));

        // Добавление HttpClient и регистрация TMDbService
        services.AddHttpClient<TMDbService>();

        return services;
    } 
    
    
    public static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
    
    
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Настройка контекста БД
        services.AddDatabaseServices(configuration);
        
        // Настройка ASP.NET Identity
        services.AddIdentityServices();
        
        // Добавляем сервисы Аутентификации и Авторизации.
        services.AddJwtAuthentication(configuration);
        
        // Метод для настройки TMDb интеграции
        services.AddTMDbIntegration(configuration);
        
        // Метод для настройки FluentValidation
        services.AddFluentValidationServices();
        
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1",new OpenApiInfo{ Title = "MovieRev API", Version = "v1"});
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"{token}\"",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
        return services;
    }
}