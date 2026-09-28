using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Procurement.Api.Data;
using Procurement.Api.Models;

namespace Procurement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly ProcurementDbContext _db;

    public PurchaseOrdersController(ProcurementDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchaseOrder>>> GetAll()
    {
        return await _db.PurchaseOrders.Include(o => o.Lines).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrder>> GetById(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();
        return order;
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseOrder>> Create(CreatePurchaseOrderRequest request)
    {
        var vendor = await _db.Vendors.FindAsync(request.VendorId);
        if (vendor is null || !vendor.IsActive)
            return BadRequest("Vendor does not exist or is inactive.");

        if (request.Lines is null || request.Lines.Count == 0)
            return BadRequest("An order needs at least one line.");

        if (request.Lines.Any(l => l.Quantity <= 0))
            return BadRequest("Every line needs a quantity greater than zero.");

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
            return BadRequest("One or more products do not exist.");

        var order = new PurchaseOrder { VendorId = request.VendorId };

        foreach (var line in request.Lines)
        {
            order.Lines.Add(new PurchaseOrderLine
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = products[line.ProductId].UnitPrice
            });
        }

        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id}/receive")]
    public async Task<ActionResult<PurchaseOrder>> Receive(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        if (order.Status == PurchaseOrderStatus.Received)
            return Conflict("This order has already been received.");

        await using var transaction = await _db.Database.BeginTransactionAsync();

        foreach (var line in order.Lines)
        {
            var product = await _db.Products.FindAsync(line.ProductId);
            product!.StockQuantity += line.Quantity;
        }

        order.Status = PurchaseOrderStatus.Received;
        order.ReceivedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return order;
    }
}