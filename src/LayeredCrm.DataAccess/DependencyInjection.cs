using LayeredCrm.DataAccess.DbContext;
using LayeredCrm.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LayeredCrm.DataAccess;

/// <summary>
/// Databases supported by the data access layer.
/// The business and presentation layers never know the concrete database.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>Local development default (single file, no infrastructure required).</summary>
    Sqlite,

    /// <summary>Used by the Docker environment (docker-compose) and production.</summary>
    PostgreSql
}

public static class DataAccessServiceCollectionExtensions
{
    /// <summary>
    /// Registers the DbContext and repositories in the dependency injection container.
    /// The provider is the only part that changes between SQLite (local) and PostgreSQL (Docker/production).
    /// </summary>
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        string connectionString,
        DatabaseProvider provider = DatabaseProvider.Sqlite)
    {
        services.AddDbContext<CrmDbContext>(options =>
        {
            if (provider == DatabaseProvider.PostgreSql)
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }

    /// <summary>
    /// Converts the "DatabaseProvider" setting (environment variable or appsettings) into the enum.
    /// Accepts "Sqlite" and "PostgreSql"/"Postgres" (case-insensitive).
    /// </summary>
    public static DatabaseProvider ParseDatabaseProvider(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "postgres" or "postgresql" or "npgsql" => DatabaseProvider.PostgreSql,
            "sqlite" or null or "" => DatabaseProvider.Sqlite,
            _ => throw new InvalidOperationException(
                $"Unknown database provider '{value}'. Use 'Sqlite' or 'PostgreSql'.")
        };
}
