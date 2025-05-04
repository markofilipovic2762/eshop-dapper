using System.ComponentModel.DataAnnotations;
namespace EshopDapper.Entities;
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? City { get; set; } = string.Empty;
    public int? PostalCode { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public string Username { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    public string Role { get; set; } = "User"; // Can be "Admin" or "User"
}