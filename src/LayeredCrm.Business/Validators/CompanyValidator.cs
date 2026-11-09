using LayeredCrm.Domain.Entities;

namespace LayeredCrm.Business.Validators;

/// <summary>
/// Validates a <see cref="Company"/>.
/// </summary>
public sealed class CompanyValidator
{
    public IReadOnlyList<string> Validate(Company company)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(company.Name))
        {
            errors.Add("Name is required.");
        }
        else if (company.Name.Trim().Length > 200)
        {
            errors.Add("Name must be at most 200 characters long.");
        }

        if (!string.IsNullOrWhiteSpace(company.Industry) && company.Industry.Trim().Length > 100)
        {
            errors.Add("Industry must be at most 100 characters long.");
        }

        return errors;
    }
}
