using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AspnetCore.Data;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDataContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(connectionString,
                b =>
                {
                    b.CommandTimeout(1200);
                    b.MigrationsAssembly("AspnetCore.Api");
                }
            );
            options.ConfigureWarnings(config =>
            {
                config.Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning);
                config.Ignore(RelationalEventId.BoolWithDefaultWarning);
            });
        }, ServiceLifetime.Transient);

        services.AddTransient<UnitOfWork>();

        return services;
    }
}