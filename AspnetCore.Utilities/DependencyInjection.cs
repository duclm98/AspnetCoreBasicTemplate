using Microsoft.Extensions.DependencyInjection;

namespace AspnetCore.Utilities;

public static class DependencyInjection
{
    public static IServiceCollection RegisterAllDependency(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x =>
                (x.FullName ?? "").Contains("AspnetCore.Utilities")
                || (x.FullName ?? "").Contains("AspnetCore.Business")
            ).ToList();
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(x =>
                    x.Name.EndsWith("Repository")
                    || x.Name.EndsWith("Service")
                    || x.Name.EndsWith("SubService")
                )
                .ToList();

            foreach (var type in types)
            {
                if (type.BaseType != null)
                {
                    var interfaceType = type.GetInterfaces().Except(type.BaseType.GetInterfaces()).FirstOrDefault();

                    if (interfaceType != null)
                    {
                        services.AddScoped(interfaceType, type);
                    }
                }
            }
        }

        return services;
    }
}