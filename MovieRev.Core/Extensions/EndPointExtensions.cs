using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MovieRev.Core.Extensions;

public static class EndPointExtensions
{
    public static IServiceCollection AddEndPoints(this IServiceCollection services)
    {
        services.AddEndPoints(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddEndPoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndPoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndPoint), type))
            .ToArray();
        
        services.TryAddEnumerable(serviceDescriptors);
        
        return services;
    }

    // Новый метод расширения для настройки HTTP-пайплайна
    public static WebApplication AddMiddleware(this WebApplication app)
    {
        // УЛУЧШЕНИЕ 2: Middleware для аутентификации и авторизации
        // Эти два middleware КРИТИЧНЫ для работы ClaimsPrincipal user в хэндлерах.
        // Они должны стоять ПЕРЕД MapEndPoints().
        app.UseAuthentication();
        app.UseAuthorization();
        
        return app;
    }

    // Новый метод для конфигурации Swagger UI
    public static WebApplication UseSwaggerMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();           // Создает JSON-файл спецификации
            app.UseSwaggerUI();         // Добавляет пользовательский интерфейс Swagger UI
            
            app.MapGet("/", () => Results.Redirect("/swagger"));
        }
        return app;
    }

    public static IApplicationBuilder MapEndPoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndPoint> endPoints = app.Services.GetRequiredService<IEnumerable<IEndPoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndPoint endPoint in endPoints)
        {
            endPoint.MapEndPoints(builder);
        }
        
        return app;
    }
}