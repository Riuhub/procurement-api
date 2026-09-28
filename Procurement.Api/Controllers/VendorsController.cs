using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Procurement.Api.Data;
using Procurement.Api.Models;

namespace Procurement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly ProcurementDbContext _db;

    public VendorsController(ProcurementDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Vendor>>> GetAll()
    {
        return await _db.Vendors.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Vendor>> GetById(int id)
    {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor is null)
            return NotFound();
        return vendor;
    }

    [HttpPost]
    public async Task<ActionResult<Vendor>> Create(Vendor vendor)
    {
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = vendor.Id }, vendor);
    }
}