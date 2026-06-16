using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GLMS.Api.Data;

// Used by EF Core CLI tooling (dotnet ef migrations) so it can build the context
// without running the application's startup pipeline.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GlmsDbContext>
{
    public GlmsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GlmsDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GLMS_Api_Db;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
            .Options;

        return new GlmsDbContext(options);
    }
}
