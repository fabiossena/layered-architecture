using LayeredCrm.Business.Exceptions;
using LayeredCrm.Business.Validators;
using LayeredCrm.DataAccess.Repositories;
using LayeredCrm.Domain.Entities;

namespace LayeredCrm.Business.Services;

/// <summary>
/// Business layer contract for contact operations.
/// </summary>
public interface IContactService
{
    Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Contact> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Contact> CreateAsync(Contact contact, CancellationToken cancellationToken = default);
    Task<Contact> UpdateAsync(Contact contact, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class ContactService : IContactService
{
    private readonly IContactRepository _contactRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ContactValidator _validator;

    public ContactService(
        IContactRepository contactRepository,
        ICompanyRepository companyRepository,
        ContactValidator validator)
    {
        _contactRepository = contactRepository;
        _companyRepository = companyRepository;
        _validator = validator;
    }

    public async Task<IReadOnlyList<Contact>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _contactRepository.GetAllAsync(cancellationToken);

    public async Task<Contact> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _contactRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Contact", id);

    public async Task<Contact> CreateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(contact, excludeContactId: null, cancellationToken);

        contact.CreatedAtUtc = DateTime.UtcNow;
        await _contactRepository.AddAsync(contact, cancellationToken);

        return contact;
    }

    public async Task<Contact> UpdateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        var existing = await _contactRepository.GetByIdAsync(contact.Id, cancellationToken)
            ?? throw new NotFoundException("Contact", contact.Id);

        await ValidateAsync(contact, contact.Id, cancellationToken);

        existing.FirstName = contact.FirstName.Trim();
        existing.LastName = contact.LastName.Trim();
        existing.Email = contact.Email.Trim();
        existing.Phone = contact.Phone?.Trim();
        existing.Status = contact.Status;
        existing.CompanyId = contact.CompanyId;

        _contactRepository.Update(existing);

        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contact", id);

        _contactRepository.Remove(contact);
    }

    private async Task ValidateAsync(Contact contact, int? excludeContactId, CancellationToken cancellationToken)
    {
        var errors = _validator.Validate(contact);
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        contact.Email = contact.Email.Trim();

        if (await _contactRepository.EmailExistsAsync(contact.Email, excludeContactId, cancellationToken))
        {
            throw new BusinessException($"A contact with email '{contact.Email}' already exists.");
        }

        if (contact.CompanyId.HasValue &&
            !await _companyRepository.ExistsAsync(contact.CompanyId.Value, cancellationToken))
        {
            throw new BusinessException($"Company '{contact.CompanyId.Value}' was not found.");
        }
    }
}
