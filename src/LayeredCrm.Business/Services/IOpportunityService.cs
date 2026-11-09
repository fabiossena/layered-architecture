using LayeredCrm.Domain.Entities;
using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Business.Services;

/// <summary>
/// Business layer contract for opportunity operations.
/// </summary>
public interface IOpportunityService
{
    Task<IReadOnlyList<Opportunity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Opportunity> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Opportunity> CreateAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    Task<Opportunity> UpdateAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    Task<Opportunity> ChangeStageAsync(int id, OpportunityStage newStage, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
