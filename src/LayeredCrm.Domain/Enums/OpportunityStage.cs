namespace LayeredCrm.Domain.Enums;

public enum OpportunityStage
{
    /// <summary>Initial opportunity qualification.</summary>
    Qualification = 0,

    /// <summary>Proposal sent to the customer.</summary>
    Proposal = 1,

    /// <summary>Currently under negotiation.</summary>
    Negotiation = 2,

    /// <summary>Closed won.</summary>
    Won = 3,

    /// <summary>Closed lost.</summary>
    Lost = 4
}
