using GLMS.Api.Data;
using GLMS.Api.Repositories;
using GLMS.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC controllers + JSON
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database (connection string can be overridden by environment variables in Docker)
builder.Services.AddDbContext<GlmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories (data-access layer)
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// Business-logic services (patterns moved to the backend)
builder.Services.AddScoped<IContractStatusObserver, ContractAuditObserver>();   // Observer
builder.Services.AddScoped<IContractWorkflowService, ContractWorkflowService>(); // State
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();

// Adapter: external ExchangeRate-API via typed HttpClient
builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateApiProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FxApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Apply migrations on startup (with a short retry so it tolerates SQL Server warming up in Docker).
ApplyMigrations(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GlmsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Database migration applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}); retrying in 5s...", attempt, maxAttempts);
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}

// Exposed so integration tests can reference the API host via WebApplicationFactory<Program>.
public partial class Program { }
