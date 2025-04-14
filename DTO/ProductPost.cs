namespace EshopDapper.DTO;

public class ProductPost
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Amount { get; set; }
    public int Sold { get; set; } = 0;
    public string? ImageUrl { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModified { get; set; }
    public string? LastModifiedBy { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int SubcategoryId { get; set; }
    public int SupplierId { get; set; }
}