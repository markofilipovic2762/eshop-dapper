namespace EshopDapper.Entities;

public class Subcategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string LastModifiedBy { get; set; } = string.Empty;
}