namespace LayeredCrm.Domain.Enums;

public enum ContactStatus
{
    /// <summary>Recently created, not reached out yet.</summary>
    New = 0,

    /// <summary>First contact has been made.</summary>
    Contacted = 1,

    /// <summary>Qualified as a lead.</summary>
    Qualified = 2,

    /// <summary>Active customer.</summary>
    Active = 3,

    /// <summary>No longer active.</summary>
    Inactive = 4
}
