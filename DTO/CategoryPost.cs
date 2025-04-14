namespace EshopDapper.DTO;

public class CategoryPost
{
    public required string Name { get; set; } = string.Empty;
    public string? CreatedBy { get; set; } = string.Empty;
}