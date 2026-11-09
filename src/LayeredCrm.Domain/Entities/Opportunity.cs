using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Domain.Entities;

/// <summary>
/// Sales opportunity linked to a contact.
/// </summary>
public class Opportunity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;

    public DateTime? ExpectedCloseDate { get; set; }

    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
