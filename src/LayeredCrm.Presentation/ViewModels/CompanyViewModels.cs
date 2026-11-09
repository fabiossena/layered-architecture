namespace LayeredCrm.Presentation.ViewModels;

// ---- Requests ----

public sealed record CreateCompanyRequest(string Name, string? Industry);

// ---- Responses ----

public sealed record CompanyResponse(
    int Id,
    string Name,
    string? Industry,
    DateTime CreatedAtUtc);
