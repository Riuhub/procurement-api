namespace Procurement.Api.Models;

public class PurchaseOrder
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedAt { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = new();
}