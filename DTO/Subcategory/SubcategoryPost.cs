namespace EshopDapper.DTO.Subcategory;

public class SubcategoryPost
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string ImageUrl { get; set; }
}