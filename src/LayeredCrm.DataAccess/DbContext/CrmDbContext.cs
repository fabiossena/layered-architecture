using LayeredCrm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredCrm.DataAccess.DbContext;

/// <summary>
/// Entity Framework Core context.
/// Data access layer: the only layer that knows the concrete database.
/// </summary>
public sealed class CrmDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }
}
