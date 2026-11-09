using LayeredCrm.DataAccess.DbContext;
using LayeredCrm.Domain.Entities;
using LayeredCrm.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LayeredCrm.DataAccess.Data;

/// <summary>
/// Seeds the database with sample data on first run (only when empty).
/// </summary>
public static class CrmDbContextSeeder
{
    public static async Task SeedAsync(CrmDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Companies.AnyAsync(cancellationToken))
        {
            return;
        }

        var acme = new Company { Name = "Acme Corporation", Industry = "Manufacturing" };
        var globex = new Company { Name = "Globex", Industry = "Technology" };

        var contact1 = new Contact
        {
            FirstName = "Maria",
            LastName = "Silva",
            Email = "maria.silva@acme.com",
            Phone = "+55 11 99999-0001",
            Status = ContactStatus.Qualified,
            Company = acme
        };

        var contact2 = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@globex.com",
            Phone = "+55 21 98888-0002",
            Status = ContactStatus.Contacted,
            Company = globex
        };

        dbContext.AddRange(acme, globex, contact1, contact2);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.AddRange(
            new Opportunity
            {
                Title = "Acme - Annual license",
                Amount = 120_000m,
                Stage = OpportunityStage.Proposal,
                ExpectedCloseDate = DateTime.UtcNow.AddDays(30),
                Contact = contact1
            },
            new Opportunity
            {
                Title = "Globex - CRM implementation",
                Amount = 80_000m,
                Stage = OpportunityStage.Qualification,
                ExpectedCloseDate = DateTime.UtcNow.AddDays(60),
                Contact = contact2
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
