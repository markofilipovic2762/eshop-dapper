using EshopDapper.Entities;

namespace EshopDapper.DTO;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Amount { get; set; }
    public int Sold { get; set; }
    public bool IsDeleted { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; } = string.Empty;

    public required Category Category { get; set; }
    public required Entities.Subcategory Subcategory { get; set; }
    public Entities.Supplier? Supplier { get; set; }
    public string[]? ImageUrls { get; set; } = Array.Empty<string>();
}