namespace EshopDapper.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string CreatedBy = string.Empty;
    public DateTime LastModified { get; set; }
    public string LastModifiedBy { get; set; } = string.Empty;
}