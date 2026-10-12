using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/restaurant-menu")]
public sealed class RestaurantMenuController : ControllerBase
{
    private readonly AppDbContext _context;

    public RestaurantMenuController(AppDbContext context) => _context = context;

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<RestaurantMenuAdminProductResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        var items = await _context.Items
            .AsNoTracking()
            .Include(item => item.Category)
            .Include(item => item.Taxes)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var menuItems = await _context.RestaurantMenuItems
            .AsNoTracking()
            .ToDictionaryAsync(menuItem => menuItem.ItemUniversalId, cancellationToken);

        return Ok(items.Select(item => ToResponse(item, menuItems.GetValueOrDefault(item.Id))).ToList());
    }

    [HttpPut("products/{productId:int}")]
    public async Task<ActionResult<RestaurantMenuAdminProductResponse>> UpdateProduct(
        int productId,
        UpdateRestaurantMenuProductRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MenuPrice is < 0) return BadRequest("El precio del menú no puede ser negativo.");

        var item = await _context.Items
            .Include(value => value.Category)
            .Include(value => value.Taxes)
            .FirstOrDefaultAsync(value => value.Id == productId, cancellationToken);
        if (item is null) return NotFound();

        var menuItem = await _context.RestaurantMenuItems
            .FirstOrDefaultAsync(value => value.ItemUniversalId == productId, cancellationToken);
        if (menuItem is null)
        {
            menuItem = new RestaurantMenuItem { ItemUniversalId = productId };
            _context.RestaurantMenuItems.Add(menuItem);
        }

        menuItem.IsAvailable = request.IsAvailable;
        menuItem.MenuPrice = request.MenuPrice;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(item, menuItem));
    }

    private static RestaurantMenuAdminProductResponse ToResponse(ItemUniversal item, RestaurantMenuItem? menuItem) =>
        new(
            item.Id,
            item.Name,
            item.SKU,
            item.Category?.Name ?? "Sin categoría",
            item.PriceWithTax,
            menuItem?.MenuPrice,
            menuItem?.IsAvailable ?? false);
}

/// <summary>Settings used to publish an inventory product in the restaurant menu.</summary>
public sealed record UpdateRestaurantMenuProductRequest(bool IsAvailable, decimal? MenuPrice);

/// <summary>Inventory product and its restaurant-menu settings.</summary>
public sealed record RestaurantMenuAdminProductResponse(
    int ProductId,
    string Name,
    string Sku,
    string CategoryName,
    decimal InventoryPrice,
    decimal? MenuPrice,
    bool IsAvailable);