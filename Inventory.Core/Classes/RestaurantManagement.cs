namespace Inventory.Core.Classes;

public sealed class RestaurantTable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; } = 4;
    public bool IsActive { get; set; } = true;
}

public sealed class RestaurantMenuItem
{
    public int Id { get; set; }
    public int ItemUniversalId { get; set; }
    public ItemUniversal Item { get; set; } = null!;
    public bool IsAvailable { get; set; } = true;
    public decimal? MenuPrice { get; set; }
}