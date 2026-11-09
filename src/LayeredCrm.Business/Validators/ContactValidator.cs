using System.Text.RegularExpressions;
using LayeredCrm.Domain.Entities;

namespace LayeredCrm.Business.Validators;

/// <summary>
/// Validates a <see cref="Contact"/> before any persistence happens.
/// </summary>
public sealed class ContactValidator
{
    // Simple, dependency-free email shape check (local@domain.tld).
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    public IReadOnlyList<string> Validate(Contact contact)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(contact.FirstName))
        {
            errors.Add("FirstName is required.");
        }
        else if (contact.FirstName.Trim().Length > 100)
        {
            errors.Add("FirstName must be at most 100 characters long.");
        }

        if (string.IsNullOrWhiteSpace(contact.LastName))
        {
            errors.Add("LastName is required.");
        }
        else if (contact.LastName.Trim().Length > 100)
        {
            errors.Add("LastName must be at most 100 characters long.");
        }

        if (string.IsNullOrWhiteSpace(contact.Email))
        {
            errors.Add("Email is required.");
        }
        else if (!IsValidEmail(contact.Email))
        {
            errors.Add($"Email '{contact.Email}' is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(contact.Phone) && contact.Phone.Trim().Length > 40)
        {
            errors.Add("Phone must be at most 40 characters long.");
        }

        return errors;
    }

    private static bool IsValidEmail(string email) =>
        EmailPattern.IsMatch(email);
}
