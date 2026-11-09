using LayeredCrm.Domain.Entities;

namespace LayeredCrm.Business.Validators;

/// <summary>
/// Validates an <see cref="Opportunity"/>.
/// </summary>
public sealed class OpportunityValidator
{
    public IReadOnlyList<string> Validate(Opportunity opportunity)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(opportunity.Title))
        {
            errors.Add("Title is required.");
        }
        else if (opportunity.Title.Trim().Length > 200)
        {
            errors.Add("Title must be at most 200 characters long.");
        }

        if (opportunity.Amount <= 0)
        {
            errors.Add("Amount must be greater than zero.");
        }

        if (opportunity.ContactId <= 0)
        {
            errors.Add("ContactId is required.");
        }

        if (opportunity.ExpectedCloseDate.HasValue &&
            opportunity.ExpectedCloseDate.Value.Date < DateTime.UtcNow.Date)
        {
            errors.Add("ExpectedCloseDate cannot be in the past.");
        }

        return errors;
    }
}
