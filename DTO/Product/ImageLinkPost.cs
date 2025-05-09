using System.ComponentModel.DataAnnotations;

namespace EshopDapper.DTO;

public class ImageLinkPost
{
    [Required]
    public required string ImageUrl { get; set; }
    [Required]
    public int ProductId { get; set; }
}