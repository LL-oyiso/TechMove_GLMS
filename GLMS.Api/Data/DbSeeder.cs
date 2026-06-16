using GLMS.Api.Models;
using GLMS.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Data;

public static class DbSeeder
{
    // Seeds a single admin user (idempotent) so the system is usable immediately after startup.
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GlmsDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var username = config["Auth:AdminUsername"] ?? "admin";
        var password = config["Auth:AdminPassword"] ?? "Admin123!";

        if (await db.Users.AnyAsync(u => u.Username == username))
        {
            return;
        }

        db.Users.Add(new User
        {
            Username = username,
            PasswordHash = hasher.Hash(password),
            Role = "Admin"
        });

        await db.SaveChangesAsync();
    }
}
