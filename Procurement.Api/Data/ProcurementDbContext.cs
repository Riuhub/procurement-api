using Microsoft.EntityFrameworkCore;
using Procurement.Api.Models;

namespace Procurement.Api.Data;

public class ProcurementDbContext : DbContext
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options)
        : base(options) { }

    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vendor>()
            .Property(v => v.Name).HasMaxLength(200).IsRequired();

        modelBuilder.Entity<Product>()
            .Property(p => p.Sku).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Product>()
            .Property(p => p.UnitPrice).HasPrecision(18, 2);
    }
}