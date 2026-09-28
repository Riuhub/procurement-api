namespace Procurement.Api.Models;

public record CreatePurchaseOrderLineRequest(int ProductId, int Quantity);

public record CreatePurchaseOrderRequest(
    int VendorId,
    List<CreatePurchaseOrderLineRequest> Lines);