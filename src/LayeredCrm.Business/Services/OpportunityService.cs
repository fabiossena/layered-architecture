using LayeredCrm.Business.Exceptions;
using LayeredCrm.Business.Rules;
using LayeredCrm.Business.Validators;
using LayeredCrm.DataAccess.Repositories;
using LayeredCrm.Domain.Entities;
using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Business.Services;

public sealed class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IContactRepository _contactRepository;
    private readonly OpportunityValidator _validator;

    public OpportunityService(
        IOpportunityRepository opportunityRepository,
        IContactRepository contactRepository,
        OpportunityValidator validator)
    {
        _opportunityRepository = opportunityRepository;
        _contactRepository = contactRepository;
        _validator = validator;
    }

    public async Task<IReadOnlyList<Opportunity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _opportunityRepository.GetAllAsync(cancellationToken);

    public async Task<Opportunity> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _opportunityRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException("Opportunity", id);

    public async Task<Opportunity> CreateAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(opportunity);
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        OpportunityRules.EnsureNewOpportunityRules(opportunity);

        if (await _contactRepository.GetByIdAsync(opportunity.ContactId, cancellationToken) is null)
        {
            throw new BusinessException($"Contact '{opportunity.ContactId}' was not found.");
        }

        opportunity.CreatedAtUtc = DateTime.UtcNow;
        opportunity.ExpectedCloseDate = NormalizeToUtc(opportunity.ExpectedCloseDate);
        await _opportunityRepository.AddAsync(opportunity, cancellationToken);

        // Reloads with the Contact navigation so callers receive a fully materialized entity.
        return await _opportunityRepository.GetByIdAsync(opportunity.Id, cancellationToken)
            ?? opportunity;
    }

    public async Task<Opportunity> UpdateAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        var existing = await _opportunityRepository.GetByIdAsync(opportunity.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity", opportunity.Id);

        var errors = _validator.Validate(opportunity);
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        if (opportunity.ContactId != existing.ContactId &&
            await _contactRepository.GetByIdAsync(opportunity.ContactId, cancellationToken) is null)
        {
            throw new BusinessException($"Contact '{opportunity.ContactId}' was not found.");
        }

        // The stage is intentionally NOT updated here: stage changes are a
        // domain decision and go exclusively through ChangeStageAsync,
        // which enforces the transition rules.
        existing.Title = opportunity.Title.Trim();
        existing.Amount = opportunity.Amount;
        existing.ExpectedCloseDate = NormalizeToUtc(opportunity.ExpectedCloseDate);
        existing.ContactId = opportunity.ContactId;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _opportunityRepository.Update(existing);

        // Reloads with the Contact navigation so callers receive a fully materialized entity.
        return await _opportunityRepository.GetByIdAsync(existing.Id, cancellationToken)
            ?? existing;
    }

    public async Task<Opportunity> ChangeStageAsync(int id, OpportunityStage newStage, CancellationToken cancellationToken = default)
    {
        var existing = await _opportunityRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Opportunity", id);

        OpportunityRules.EnsureValidTransition(existing.Stage, newStage);

        existing.Stage = newStage;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _opportunityRepository.Update(existing);

        return existing;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Opportunity", id);

        _opportunityRepository.Remove(opportunity);
    }

    /// <summary>
    /// Timestamps are always stored as UTC (PostgreSQL rejects non-UTC DateTime values).
    /// Plain dates without an offset are treated as UTC.
    /// </summary>
    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }
}
