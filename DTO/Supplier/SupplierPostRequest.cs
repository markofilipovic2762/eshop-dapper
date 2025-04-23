using System.ComponentModel.DataAnnotations;

namespace EshopDapper.DTO.Supplier;

public class SupplierPostRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
}