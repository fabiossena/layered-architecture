using LayeredCrm.Business.Rules;
using LayeredCrm.Business.Services;
using LayeredCrm.Domain.Entities;
using LayeredCrm.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LayeredCrm.Presentation.Controllers;

/// <summary>
/// Opportunity endpoints, including the controlled stage transition
/// (business rule enforced by the Business layer).
/// </summary>
[ApiController]
[Route("api/opportunities")]
[Produces("application/json")]
public sealed class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _opportunityService;

    public OpportunitiesController(IOpportunityService opportunityService)
    {
        _opportunityService = opportunityService;
    }

    /// <summary>Lists all opportunities (including the weighted pipeline value).</summary>
    [HttpGet]
    [ProducesResponseType<List<OpportunityResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OpportunityResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var opportunities = await _opportunityService.GetAllAsync(cancellationToken);
        return Ok(opportunities.Select(ToResponse));
    }

    /// <summary>Gets an opportunity by id.</summary>
    [HttpGet("{id:int}", Name = "GetOpportunityById")]
    [ProducesResponseType<OpportunityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OpportunityResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityService.GetByIdAsync(id, cancellationToken);
        return Ok(ToResponse(opportunity));
    }

    /// <summary>Creates a new opportunity (always starts at the Qualification stage).</summary>
    [HttpPost]
    [ProducesResponseType<OpportunityResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OpportunityResponse>> Create(
        [FromBody] CreateOpportunityRequest request,
        CancellationToken cancellationToken)
    {
        var opportunity = new Opportunity
        {
            Title = request.Title,
            Amount = request.Amount,
            ContactId = request.ContactId,
            ExpectedCloseDate = request.ExpectedCloseDate
        };

        var created = await _opportunityService.CreateAsync(opportunity, cancellationToken);

        return CreatedAtRoute("GetOpportunityById", new { id = created.Id }, ToResponse(created));
    }

    /// <summary>Updates opportunity data (title, amount, contact and expected close date).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<OpportunityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OpportunityResponse>> Update(
        int id,
        [FromBody] UpdateOpportunityRequest request,
        CancellationToken cancellationToken)
    {
        var opportunity = new Opportunity
        {
            Id = id,
            Title = request.Title,
            Amount = request.Amount,
            ContactId = request.ContactId,
            ExpectedCloseDate = request.ExpectedCloseDate
        };

        var updated = await _opportunityService.UpdateAsync(opportunity, cancellationToken);
        return Ok(ToResponse(updated));
    }

    /// <summary>Moves the opportunity forward/backward respecting the stage transition rules.</summary>
    [HttpPut("{id:int}/stage")]
    [ProducesResponseType<OpportunityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OpportunityResponse>> ChangeStage(
        int id,
        [FromBody] ChangeStageRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _opportunityService.ChangeStageAsync(id, request.Stage, cancellationToken);
        return Ok(ToResponse(updated));
    }

    /// <summary>Deletes an opportunity.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _opportunityService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static OpportunityResponse ToResponse(Opportunity opportunity) => new(
        opportunity.Id,
        opportunity.Title,
        opportunity.Amount,
        opportunity.Stage,
        OpportunityRules.CalculateWeightedAmount(opportunity),
        opportunity.ExpectedCloseDate,
        opportunity.ContactId,
        opportunity.Contact?.FullName ?? string.Empty,
        opportunity.CreatedAtUtc,
        opportunity.UpdatedAtUtc);
}
