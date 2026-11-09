using LayeredCrm.Business.Services;
using LayeredCrm.Domain.Entities;
using LayeredCrm.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LayeredCrm.Presentation.Controllers;

/// <summary>
/// Presentation layer: receives the HTTP request, maps it to the domain model,
/// delegates to the business layer and formats the response.
/// </summary>
[ApiController]
[Route("api/contacts")]
[Produces("application/json")]
public sealed class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>Lists all contacts.</summary>
    [HttpGet]
    [ProducesResponseType<List<ContactResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContactResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var contacts = await _contactService.GetAllAsync(cancellationToken);
        return Ok(contacts.Select(ToResponse));
    }

    /// <summary>Gets a contact by id.</summary>
    [HttpGet("{id:int}", Name = "GetContactById")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var contact = await _contactService.GetByIdAsync(id, cancellationToken);
        return Ok(ToResponse(contact));
    }

    /// <summary>Creates a new contact.</summary>
    [HttpPost]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContactResponse>> Create(
        [FromBody] CreateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Status = request.Status,
            CompanyId = request.CompanyId
        };

        var created = await _contactService.CreateAsync(contact, cancellationToken);

        return CreatedAtRoute("GetContactById", new { id = created.Id }, ToResponse(created));
    }

    /// <summary>Updates an existing contact.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContactResponse>> Update(
        int id,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = new Contact
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Status = request.Status,
            CompanyId = request.CompanyId
        };

        var updated = await _contactService.UpdateAsync(contact, cancellationToken);
        return Ok(ToResponse(updated));
    }

    /// <summary>Deletes a contact.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _contactService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static ContactResponse ToResponse(Contact contact) => new(
        contact.Id,
        contact.FirstName,
        contact.LastName,
        contact.FullName,
        contact.Email,
        contact.Phone,
        contact.Status,
        contact.CompanyId,
        contact.Company?.Name,
        contact.CreatedAtUtc);
}
