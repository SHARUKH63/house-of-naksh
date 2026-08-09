namespace HouseOfNaksh.Domain;

public class Order
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public required string CustomerEmail { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime PlacedAtUtc { get; set; }

    public List<OrderItem> Items { get; set; } = [];

    public decimal TotalAmount => Items.Sum(i => i.LineTotal);
}