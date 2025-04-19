namespace EshopDapper.Entities;

public class Shipper
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; } = string.Empty;
}