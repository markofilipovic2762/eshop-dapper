namespace EshopDapper.DTO;

public class OrderResultDto{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public int? EmployeeId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string ShipCity { get; set; } = string.Empty;
    public int? ShipPostalCode { get; set; }

    public List<OrderItemResult> Items { get; set; } = [];
}