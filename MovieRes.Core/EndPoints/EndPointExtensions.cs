using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MovieRes.Core.EndPoints;

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

    public static IApplicationBuilder MapEndPoints(this WebApplication app, RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndPoint> endPoints = app.Services.GetRequiredService<IEnumerable<IEndPoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndPoint endPoint in endPoints)
        {
            endPoint.MapEndPoint(builder);
        }
        
        return app;
    }
}