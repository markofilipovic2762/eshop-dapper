namespace EshopDapper.DTO;

public class OrderFull
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string EmployeeName { get; set; }
    public string ShipperName { get; set; }
    public int? EmployeeId { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? ShippedDate { get; set; }
    public int? ShipperId { get; set; }
    public string ShipCity { get; set; }
    public int? ShipPostalCode { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string ShipAddress { get; set; }
    public List<OrderItemFull> OrderDetails { get; set; } = new();
}