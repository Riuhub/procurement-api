using Microsoft.EntityFrameworkCore;
using Procurement.Api.Models;

namespace Procurement.Api.Data;

public class ProcurementDbContext : DbContext
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options)
        : base(options) { }

    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

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
        modelBuilder.Entity<PurchaseOrder>()
            .Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne<Vendor>().WithMany().HasForeignKey(o => o.VendorId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PurchaseOrder>()
            .HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PurchaseOrderLine>()
            .HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PurchaseOrderLine>()
            .Property(l => l.UnitPrice).HasPrecision(18, 2);
    }
}