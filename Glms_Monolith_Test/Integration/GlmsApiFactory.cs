using GLMS.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Glms_Monolith_Test.Integration;

// Boots the real GLMS.Api in-process for integration testing, but swaps SQL Server
// for an isolated in-memory database so the tests need no external dependencies.
public class GlmsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"glms-itests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GlmsDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<GlmsDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
