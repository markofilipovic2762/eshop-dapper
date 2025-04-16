namespace EshopDapper.DTO;

public class OrderResultDto{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string ShipAddress { get; set; } = string.Empty;
    public string ShipCity { get; set; } = string.Empty;
    public int? ShipPostalCode { get; set; }

    public List<OrderItemResult> Items { get; set; } = [];
}