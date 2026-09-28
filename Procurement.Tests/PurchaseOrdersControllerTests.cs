using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Procurement.Api.Controllers;
using Procurement.Api.Data;
using Procurement.Api.Models;

namespace Procurement.Tests;

public class PurchaseOrdersControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ProcurementDbContext _db;
    private readonly PurchaseOrdersController _controller;

    // Runs before every test: fresh empty database with one vendor and one product
    public PurchaseOrdersControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ProcurementDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new ProcurementDbContext(options);
        _db.Database.EnsureCreated();

        _db.Vendors.Add(new Vendor { Name = "Acme Supplies" });
        _db.Products.Add(new Product
        {
            Sku = "LAP-001",
            Name = "Laptop",
            UnitPrice = 899.99m,
            StockQuantity = 0
        });
        _db.SaveChanges();

        _controller = new PurchaseOrdersController(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<int> CreateDraftOrder(int quantity)
    {
        var request = new CreatePurchaseOrderRequest(
            1,
            new List<CreatePurchaseOrderLineRequest>
            {
                new CreatePurchaseOrderLineRequest(1, quantity)
            });

        var result = await _controller.Create(request);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        return ((PurchaseOrder)created.Value!).Id;
    }

    [Fact]
    public async Task Receive_AddsStockAndMarksOrderReceived()
    {
        var orderId = await CreateDraftOrder(10);

        var result = await _controller.Receive(orderId);

        Assert.Equal(PurchaseOrderStatus.Received, result.Value!.Status);
        Assert.Equal(10, _db.Products.Single().StockQuantity);
    }

    [Fact]
    public async Task Receive_SameOrderTwice_IsRejectedAndStockNotDoubled()
    {
        var orderId = await CreateDraftOrder(10);
        await _controller.Receive(orderId);

        var second = await _controller.Receive(orderId);

        Assert.IsType<ConflictObjectResult>(second.Result);
        Assert.Equal(10, _db.Products.Single().StockQuantity);
    }

    [Fact]
    public async Task Create_WithZeroQuantity_ReturnsBadRequest()
    {
        var request = new CreatePurchaseOrderRequest(
            1,
            new List<CreatePurchaseOrderLineRequest>
            {
                new CreatePurchaseOrderLineRequest(1, 0)
            });

        var result = await _controller.Create(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithUnknownVendor_ReturnsBadRequest()
    {
        var request = new CreatePurchaseOrderRequest(
            999,
            new List<CreatePurchaseOrderLineRequest>
            {
                new CreatePurchaseOrderLineRequest(1, 5)
            });

        var result = await _controller.Create(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}