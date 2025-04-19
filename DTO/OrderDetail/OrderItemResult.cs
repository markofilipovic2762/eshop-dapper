namespace EshopDapper.DTO;

public class OrderItemResult
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal OrderPrice { get; set; }
    public int Quantity { get; set; }
    public decimal? Discount { get; set; }
}