using GLMS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Data;

public class GlmsDbContext : DbContext
{
    public GlmsDbContext(DbContextOptions<GlmsDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Client)
            .WithMany(c => c.Contracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Contract)
            .WithMany(c => c.ServiceRequests)
            .HasForeignKey(sr => sr.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>().Property(x => x.CostUsd).HasPrecision(18, 2);
        modelBuilder.Entity<ServiceRequest>().Property(x => x.CostZar).HasPrecision(18, 2);
        modelBuilder.Entity<ServiceRequest>().Property(x => x.ExchangeRateUsed).HasPrecision(18, 6);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}
