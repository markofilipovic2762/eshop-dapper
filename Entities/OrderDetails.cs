namespace EshopDapper.Entities;

public class OrderDetails
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? Discount { get; set; } = 0;
    public DateTime? Created { get; set; }
}