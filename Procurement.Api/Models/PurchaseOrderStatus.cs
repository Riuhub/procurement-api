using System.Text.Json.Serialization;

namespace Procurement.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PurchaseOrderStatus
{
    Draft,
    Received
}