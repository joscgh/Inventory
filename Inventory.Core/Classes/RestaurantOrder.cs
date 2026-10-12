namespace Inventory.Core.Classes;

public enum RestaurantOrderStatus
{
    New,
    Preparing,
    Ready,
    Served,
    Closed
}

public sealed class RestaurantOrder
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public string WaiterName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public RestaurantOrderStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public List<RestaurantOrderLine> Lines { get; set; } = [];
}

public sealed class RestaurantOrderLine
{
    public int Id { get; set; }
    public int RestaurantOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}