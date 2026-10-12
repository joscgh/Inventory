namespace Inventory.Restaurant.Models;

public sealed record MenuProduct(int Id, string Name, string Sku, string CategoryName, decimal Price);

public enum RestaurantOrderStatus
{
    New,
    Preparing,
    Ready,
    Served,
    Closed
}

public sealed record RestaurantOrderLine(
    int Id,
    int ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);

public sealed record RestaurantOrder(
    int Id,
    string TableName,
    int GuestCount,
    string WaiterName,
    string? Notes,
    RestaurantOrderStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<RestaurantOrderLine> Lines,
    decimal Total);

public sealed record CreateRestaurantOrderLine(int ProductId, decimal Quantity, string? Notes);
public sealed record CreateRestaurantOrder(string TableName, int GuestCount, string WaiterName, string? Notes, List<CreateRestaurantOrderLine> Lines);
public sealed record AddRestaurantOrderLines(List<CreateRestaurantOrderLine> Lines);
public sealed record AdvanceRestaurantOrderStatus(RestaurantOrderStatus Status);

public sealed record RestaurantTable(int Id, string Name, int Capacity, bool IsActive);
public sealed record CreateRestaurantTable(string Name, int Capacity);
public sealed record UpdateRestaurantTable(string Name, int Capacity, bool IsActive);
public sealed record RestaurantMenuAdminProduct(
    int ProductId,
    string Name,
    string Sku,
    string CategoryName,
    decimal InventoryPrice,
    decimal? MenuPrice,
    bool IsAvailable);
public sealed record UpdateRestaurantMenuProduct(bool IsAvailable, decimal? MenuPrice);