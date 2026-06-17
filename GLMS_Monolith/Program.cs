using GLMS_Monolith.Filters;
using GLMS_Monolith.Services.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// MVC + global API-exception handling + "must be signed in" policy for every page.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiExceptionFilter>();

    var requireAuth = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(requireAuth));
});

builder.Services.AddHttpContextAccessor();

// Cookie authentication gates the MVC pages (the JWT itself is held in session for API calls).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Session holds the signed-in user's JWT for outgoing API calls.
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
