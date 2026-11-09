using LayeredCrm.DataAccess.DbContext;
using LayeredCrm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredCrm.DataAccess.Repositories;

public interface ICompanyRepository
{
    Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
}

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly CrmDbContext _dbContext;

    public CompanyRepository(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Companies
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public Task<Company?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Companies.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return _dbContext.Companies.AnyAsync(
            c => c.Name.ToLower() == normalized,
            cancellationToken);
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _dbContext.Companies.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
