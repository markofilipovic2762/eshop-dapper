using System.ComponentModel.DataAnnotations;

namespace EshopDapper.Entities;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    [EmailAddress]
    public string? Email { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.Now;
}