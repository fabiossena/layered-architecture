using LayeredCrm.Business.Exceptions;
using LayeredCrm.Business.Validators;
using LayeredCrm.DataAccess.Repositories;
using LayeredCrm.Domain.Entities;

namespace LayeredCrm.Business.Services;

/// <summary>
/// Business layer contract for company operations.
/// </summary>
public interface ICompanyService
{
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Company> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Company> CreateAsync(Company company, CancellationToken cancellationToken = default);
}

public sealed class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly CompanyValidator _validator;

    public CompanyService(ICompanyRepository companyRepository, CompanyValidator validator)
    {
        _companyRepository = companyRepository;
        _validator = validator;
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _companyRepository.GetAllAsync(cancellationToken);

    public async Task<Company> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _companyRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Company", id);

    public async Task<Company> CreateAsync(Company company, CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(company);
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        company.Name = company.Name.Trim();
        company.Industry = company.Industry?.Trim();

        if (await _companyRepository.NameExistsAsync(company.Name, cancellationToken))
        {
            throw new BusinessException($"A company named '{company.Name}' already exists.");
        }

        company.CreatedAtUtc = DateTime.UtcNow;
        await _companyRepository.AddAsync(company, cancellationToken);

        return company;
    }
}
