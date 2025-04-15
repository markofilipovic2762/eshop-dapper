namespace EshopDapper.DTO;

public class OrderCreateDto
{
    public int UserId { get; set; }
    public int? EmployeeId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string ShipCity { get; set; } = string.Empty;
    public int? ShipPostalCode { get; set; }

    public List<OrderItemDto> Items { get; set; } = [];
}