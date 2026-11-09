using System.Reflection;
using System.Text.Json.Serialization;
using LayeredCrm.Business.Services;
using LayeredCrm.Business.Validators;
using LayeredCrm.DataAccess;
using LayeredCrm.DataAccess.Data;
using LayeredCrm.DataAccess.DbContext;
using LayeredCrm.Presentation.Middleware;

// ---- Web UI resolution -------------------------------------------------
// The web client lives in its own folder (src/LayeredCrm.Web), structurally
// outside the layered projects. It is a separate client that only talks to
// this API over HTTP, but is served from the same host for demo convenience.
// Resolved before CreateBuilder because the web root must be provided via
// WebApplicationOptions (it cannot be changed afterwards).
static string? FindWebClientPath()
{
    var candidates = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), "LayeredCrm.Web"),           // docker: /app
        Path.Combine(Directory.GetCurrentDirectory(), "..", "LayeredCrm.Web"),     // dotnet run from src/LayeredCrm.Presentation
        Path.Combine(Directory.GetCurrentDirectory(), "src", "LayeredCrm.Web"),    // dotnet run from repo root
        Path.Combine(Directory.GetCurrentDirectory(), "..", "src", "LayeredCrm.Web"),
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "LayeredCrm.Web") // bin/<config>/net9.0
    };

    return candidates.Select(Path.GetFullPath).FirstOrDefault(Directory.Exists);
}

var webClientPath = FindWebClientPath();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = webClientPath // null => default wwwroot behaviour
});

if (webClientPath is null)
{
    Console.WriteLine("[LayeredCrm.Web] WARNING: web client folder not found — only Swagger will be available.");
}

// ---- Presentation layer: dependency composition (composition root) ----
// Enums are exposed as strings ("Qualification") instead of raw numbers.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// CORS so the UI can also be opened from another origin (e.g. a static file server).
builder.Services.AddCors(options => options.AddPolicy("WebClient", policy =>
    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Swagger UI at /swagger (interactive documentation for every endpoint).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Data access layer (repositories + EF Core).
// Local: SQLite (default). Docker/production: PostgreSQL via environment variables.
var connectionString = builder.Configuration.GetConnectionString("Crm")
    ?? "Data Source=crm.db";

var databaseProvider = DataAccessServiceCollectionExtensions.ParseDatabaseProvider(
    builder.Configuration["DatabaseProvider"]);

builder.Services.AddDataAccess(connectionString, databaseProvider);

// Business layer (services + validators).
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ContactValidator>();
builder.Services.AddScoped<OpportunityValidator>();
builder.Services.AddScoped<CompanyValidator>();

var app = builder.Build();

// Creates the schema and seeds sample data on first run.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    await CrmDbContextSeeder.SeedAsync(dbContext);
}

// Maps exceptions from the lower layers into proper HTTP responses.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("WebClient");

// Interactive API documentation (Swagger UI) — part of the Presentation layer.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Layered CRM API v1");
    options.DocumentTitle = "Layered CRM API";
});

// LayeredCrm.Web (../LayeredCrm.Web) — a structurally separate static client.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
