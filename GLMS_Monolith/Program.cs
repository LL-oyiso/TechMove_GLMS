using GLMS_Monolith.Filters;
using GLMS_Monolith.Services.Api;

var builder = WebApplication.CreateBuilder(args);

// MVC + global API-exception handling.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
});

builder.Services.AddHttpContextAccessor();

// Session is used to hold the signed-in user's JWT for outgoing API calls.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<AuthTokenHandler>();

// Base URL of the GLMS API (overridden by the ApiBaseUrl environment variable in Docker).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5206";

// Auth client does not need a bearer token (it is how you obtain one).
builder.Services.AddHttpClient<IAuthApi, AuthApi>(client => client.BaseAddress = new Uri(apiBaseUrl));

// Business clients attach the JWT via the AuthTokenHandler.
builder.Services.AddHttpClient<IClientsApi, ClientsApi>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IContractsApi, ContractsApi>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddHttpClient<IServiceRequestsApi, ServiceRequestsApi>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
