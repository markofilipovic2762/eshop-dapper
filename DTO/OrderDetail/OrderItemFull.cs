using EshopDapper.Entities;

namespace EshopDapper.DTO;

public class OrderItemFull
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal? Price { get; set; }
    public int Quantity { get; set; }
    public DateTime Created { get; set; }

    public Product Product { get; set; } = new();
}