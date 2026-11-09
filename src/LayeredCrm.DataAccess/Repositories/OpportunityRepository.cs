using LayeredCrm.DataAccess.DbContext;
using LayeredCrm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredCrm.DataAccess.Repositories;

public interface IOpportunityRepository
{
    Task<List<Opportunity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Opportunity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    void Update(Opportunity opportunity);
    void Remove(Opportunity opportunity);
}

public sealed class OpportunityRepository : IOpportunityRepository
{
    private readonly CrmDbContext _dbContext;

    public OpportunityRepository(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Opportunity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Opportunities
            .AsNoTracking()
            .Include(o => o.Contact)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<Opportunity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Opportunities
            .Include(o => o.Contact)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Opportunities.AddAsync(opportunity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Update(Opportunity opportunity)
    {
        _dbContext.Opportunities.Update(opportunity);
        _dbContext.SaveChanges();
    }

    public void Remove(Opportunity opportunity)
    {
        _dbContext.Opportunities.Remove(opportunity);
        _dbContext.SaveChanges();
    }
}
