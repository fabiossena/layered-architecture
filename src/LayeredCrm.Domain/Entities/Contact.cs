using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Domain.Entities;

/// <summary>
/// CRM contact (person) aggregate root.
/// </summary>
public class Contact
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.New;

    /// <summary>Optional company the contact belongs to.</summary>
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";
}
