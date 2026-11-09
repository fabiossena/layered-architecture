using LayeredCrm.Business.Exceptions;
using LayeredCrm.Domain.Entities;
using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Business.Rules;

/// <summary>
/// Pure opportunity rules: stage transitions and weighted value.
/// No data access — business logic only.
/// </summary>
public static class OpportunityRules
{
    private static readonly IReadOnlyDictionary<OpportunityStage, IReadOnlySet<OpportunityStage>> AllowedTransitions =
        new Dictionary<OpportunityStage, IReadOnlySet<OpportunityStage>>
        {
            [OpportunityStage.Qualification] = new HashSet<OpportunityStage>
            {
                OpportunityStage.Proposal, OpportunityStage.Lost
            },
            [OpportunityStage.Proposal] = new HashSet<OpportunityStage>
            {
                OpportunityStage.Negotiation, OpportunityStage.Lost
            },
            [OpportunityStage.Negotiation] = new HashSet<OpportunityStage>
            {
                OpportunityStage.Won, OpportunityStage.Lost
            },
            [OpportunityStage.Won] = new HashSet<OpportunityStage>(),
            [OpportunityStage.Lost] = new HashSet<OpportunityStage>()
        };

    /// <summary>Weight of each stage over the total value (weighted pipeline).</summary>
    private static readonly IReadOnlyDictionary<OpportunityStage, decimal> StageWeights =
        new Dictionary<OpportunityStage, decimal>
        {
            [OpportunityStage.Qualification] = 0.25m,
            [OpportunityStage.Proposal] = 0.50m,
            [OpportunityStage.Negotiation] = 0.80m,
            [OpportunityStage.Won] = 1.00m,
            [OpportunityStage.Lost] = 0.00m
        };

    public static bool CanTransition(OpportunityStage from, OpportunityStage to) =>
        from == to || AllowedTransitions[from].Contains(to);

    /// <summary>
    /// Throws <see cref="BusinessException"/> when the stage transition is not allowed.
    /// </summary>
    public static void EnsureValidTransition(OpportunityStage from, OpportunityStage to)
    {
        if (!CanTransition(from, to))
        {
            throw new BusinessException(
                $"Invalid stage transition: {from} -> {to}. " +
                $"Allowed stages from '{from}': {string.Join(", ", AllowedTransitions[from])}.");
        }
    }

    /// <summary>Opportunity value weighted by its current stage.</summary>
    public static decimal CalculateWeightedAmount(Opportunity opportunity) =>
        Math.Round(opportunity.Amount * StageWeights[opportunity.Stage], 2);

    /// <summary>A new opportunity always starts at the Qualification stage.</summary>
    public static void EnsureNewOpportunityRules(Opportunity opportunity)
    {
        if (opportunity.Stage != OpportunityStage.Qualification)
        {
            throw new BusinessException("A new opportunity must start at the 'Qualification' stage.");
        }
    }
}
