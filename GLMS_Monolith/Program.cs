
using GLMS_Monolith.Services;
using GLMS_Monolith.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();








builder.Services.AddDbContext<GlmsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContractWorkflowService, ContractWorkflowService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

builder.Services.AddHttpClient<IExchangeRateProvider, ExchangeRateApiProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["FxApi:BaseUrl"] ?? "https://v6.exchangerate-api.com/v6/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
