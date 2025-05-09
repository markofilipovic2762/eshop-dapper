namespace EshopDapper.DTO;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime LastModified { get; set; }
    public string LastModifiedBy { get; set; }
    public string ImageUrl { get; set; }
    public List<Entities.Subcategory> Subcategories { get; set; } = new();
}