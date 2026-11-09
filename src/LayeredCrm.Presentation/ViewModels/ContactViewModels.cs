using LayeredCrm.Domain.Enums;

namespace LayeredCrm.Presentation.ViewModels;

// ---- Requests ----

public sealed record CreateContactRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    ContactStatus Status,
    int? CompanyId);

public sealed record UpdateContactRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    ContactStatus Status,
    int? CompanyId);

// ---- Responses ----

public sealed record ContactResponse(
    int Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? Phone,
    ContactStatus Status,
    int? CompanyId,
    string? CompanyName,
    DateTime CreatedAtUtc);
