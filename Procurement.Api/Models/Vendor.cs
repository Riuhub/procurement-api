using System.ComponentModel.DataAnnotations;

namespace Procurement.Api.Models;

public class Vendor
{
    public int Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
}