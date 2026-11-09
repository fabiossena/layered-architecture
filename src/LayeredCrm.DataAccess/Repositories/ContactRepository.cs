using LayeredCrm.DataAccess.DbContext;
using LayeredCrm.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredCrm.DataAccess.Repositories;

public interface IContactRepository
{
    Task<List<Contact>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Contact?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludeContactId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    void Update(Contact contact);
    void Remove(Contact contact);
}

public sealed class ContactRepository : IContactRepository
{
    private readonly CrmDbContext _dbContext;

    public ContactRepository(CrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Contact>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _dbContext.Contacts
            .AsNoTracking()
            .Include(c => c.Company)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Contact?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Contacts
            .AsNoTracking()
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludeContactId = null, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return _dbContext.Contacts.AnyAsync(
            c => c.Email.ToLower() == normalized && (!excludeContactId.HasValue || c.Id != excludeContactId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await _dbContext.Contacts.AddAsync(contact, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Update(Contact contact)
    {
        _dbContext.Contacts.Update(contact);
        _dbContext.SaveChanges();
    }

    public void Remove(Contact contact)
    {
        _dbContext.Contacts.Remove(contact);
        _dbContext.SaveChanges();
    }
}
