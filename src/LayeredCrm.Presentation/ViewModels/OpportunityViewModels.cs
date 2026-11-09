using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Presentation.ViewModels;

// ---- Requests ----

public sealed record CreateOpportunityRequest(
    string Title,
    decimal Amount,
    int ContactId,
    DateTime? ExpectedCloseDate);

public sealed record UpdateOpportunityRequest(
    string Title,
    decimal Amount,
    int ContactId,
    DateTime? ExpectedCloseDate);

public sealed record ChangeStageRequest(OpportunityStage Stage);

// ---- Responses ----

public sealed record OpportunityResponse(
    int Id,
    string Title,
    decimal Amount,
    OpportunityStage Stage,
    decimal WeightedAmount,
    DateTime? ExpectedCloseDate,
    int ContactId,
    string ContactName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
