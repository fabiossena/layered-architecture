using LayeredCrm.Business.Services;
using LayeredCrm.Domain.Entities;
using LayeredCrm.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LayeredCrm.Presentation.Controllers;

/// <summary>
/// Company endpoints (used by contacts and the web UI).
/// </summary>
[ApiController]
[Route("api/companies")]
[Produces("application/json")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    /// <summary>Lists all companies.</summary>
    [HttpGet]
    [ProducesResponseType<List<CompanyResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetAllAsync(cancellationToken);
        return Ok(companies.Select(ToResponse));
    }

    /// <summary>Gets a company by id.</summary>
    [HttpGet("{id:int}", Name = "GetCompanyById")]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var company = await _companyService.GetByIdAsync(id, cancellationToken);
        return Ok(ToResponse(company));
    }

    /// <summary>Creates a new company.</summary>
    [HttpPost]
    [ProducesResponseType<CompanyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyResponse>> Create(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var company = new Company
        {
            Name = request.Name,
            Industry = request.Industry
        };

        var created = await _companyService.CreateAsync(company, cancellationToken);

        return CreatedAtRoute("GetCompanyById", new { id = created.Id }, ToResponse(created));
    }

    private static CompanyResponse ToResponse(Company company) =>
        new(company.Id, company.Name, company.Industry, company.CreatedAtUtc);
}
