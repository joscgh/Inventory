using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/restaurant-orders")]
public sealed class RestaurantOrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public RestaurantOrdersController(AppDbContext context) => _context = context;

    [HttpGet("menu")]
    public async Task<ActionResult<IReadOnlyList<MenuItemResponse>>> GetMenu(CancellationToken cancellationToken)
    {
        var items = await _context.RestaurantMenuItems
            .AsNoTracking()
            .Where(menuItem => menuItem.IsAvailable)
            .Include(menuItem => menuItem.Item)
                .ThenInclude(item => item.Category)
            .Include(menuItem => menuItem.Item)
                .ThenInclude(item => item.Taxes)
            .OrderBy(menuItem => menuItem.Item.Name)
            .ToListAsync(cancellationToken);

        var menu = items.Select(menuItem => new MenuItemResponse(
                menuItem.Item.Id,
                menuItem.Item.Name,
                menuItem.Item.SKU,
                menuItem.Item.Category == null ? "Sin categoría" : menuItem.Item.Category.Name,
                menuItem.MenuPrice ?? menuItem.Item.PriceWithTax))
            .ToList();

        return Ok(menu);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RestaurantOrderResponse>>> GetActiveOrders(CancellationToken cancellationToken)
    {
        var orders = await _context.RestaurantOrders
            .AsNoTracking()
            .Include(order => order.Lines)
            .Where(order => order.Status != RestaurantOrderStatus.Closed)
            .OrderByDescending(order => order.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(orders.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RestaurantOrderResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await _context.RestaurantOrders
            .AsNoTracking()
            .Include(value => value.Lines)
            .FirstOrDefaultAsync(value => value.Id == id, cancellationToken);

        return order is null ? NotFound() : Ok(ToResponse(order));
    }

    [HttpPost]
    public async Task<ActionResult<RestaurantOrderResponse>> Create(
        CreateRestaurantOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TableName)
            || string.IsNullOrWhiteSpace(request.WaiterName)
            || request.GuestCount is < 1 or > 30
            || request.Lines is null
            || request.Lines.Count == 0)
        {
            return BadRequest("Indica mesa, mesero, comensales y al menos un producto.");
        }

        var tableName = request.TableName.Trim();
        var table = await _context.RestaurantTables
            .FirstOrDefaultAsync(value => value.Name == tableName && value.IsActive, cancellationToken);
        if (table is null)
            return BadRequest("La mesa no existe o está desactivada.");
        if (request.GuestCount > table.Capacity)
            return BadRequest("La cantidad de comensales supera la capacidad de la mesa.");

        if (await _context.RestaurantOrders.AnyAsync(
                order => order.TableName == tableName && order.Status != RestaurantOrderStatus.Closed,
                cancellationToken))
        {
            return Conflict("La mesa ya tiene una comanda abierta.");
        }

        var lines = await CreateLines(request.Lines, cancellationToken);
        if (lines is null) return BadRequest("Uno o más productos no existen o tienen una cantidad inválida.");

        var now = DateTimeOffset.UtcNow;
        var order = new RestaurantOrder
        {
            TableName = tableName,
            GuestCount = request.GuestCount,
            WaiterName = request.WaiterName.Trim(),
            Notes = CleanNotes(request.Notes),
            Status = RestaurantOrderStatus.New,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Lines = lines
        };

        _context.RestaurantOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToResponse(order));
    }

    [HttpPost("{id:int}/lines")]
    public async Task<ActionResult<RestaurantOrderResponse>> AddLines(
        int id,
        AddRestaurantOrderLinesRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _context.RestaurantOrders
            .Include(value => value.Lines)
            .FirstOrDefaultAsync(value => value.Id == id, cancellationToken);

        if (order is null) return NotFound();
        if (order.Status >= RestaurantOrderStatus.Ready)
            return Conflict("No se pueden agregar productos después de que la comanda esté lista.");
        if (request.Lines is null || request.Lines.Count == 0)
            return BadRequest("Agrega al menos un producto.");

        var lines = await CreateLines(request.Lines, cancellationToken);
        if (lines is null) return BadRequest("Uno o más productos no existen o tienen una cantidad inválida.");

        order.Lines.AddRange(lines);
        order.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(order));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<RestaurantOrderResponse>> AdvanceStatus(
        int id,
        AdvanceRestaurantOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _context.RestaurantOrders
            .Include(value => value.Lines)
            .FirstOrDefaultAsync(value => value.Id == id, cancellationToken);

        if (order is null) return NotFound();
        if (!Enum.IsDefined(request.Status) || (int)request.Status != (int)order.Status + 1)
            return BadRequest("La comanda debe avanzar un estado a la vez.");

        order.Status = request.Status;
        order.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(order));
    }

    private async Task<List<RestaurantOrderLine>?> CreateLines(
        IReadOnlyList<CreateRestaurantOrderLineRequest> requests,
        CancellationToken cancellationToken)
    {
        if (requests.Any(line => line.ProductId <= 0 || line.Quantity <= 0)) return null;

        var productIds = requests.Select(line => line.ProductId).Distinct().ToArray();
        var products = await _context.RestaurantMenuItems
            .AsNoTracking()
            .Where(menuItem => menuItem.IsAvailable && productIds.Contains(menuItem.ItemUniversalId))
            .Include(menuItem => menuItem.Item)
                .ThenInclude(item => item.Taxes)
            .ToDictionaryAsync(menuItem => menuItem.ItemUniversalId, cancellationToken);

        if (products.Count != productIds.Length) return null;

        return requests.Select(line => new RestaurantOrderLine
        {
            ProductId = line.ProductId,
            ProductName = products[line.ProductId].Item.Name,
            Quantity = line.Quantity,
            UnitPrice = products[line.ProductId].MenuPrice ?? products[line.ProductId].Item.PriceWithTax,
            Notes = CleanNotes(line.Notes)
        }).ToList();
    }

    private static RestaurantOrderResponse ToResponse(RestaurantOrder order)
    {
        var lines = order.Lines
            .OrderBy(line => line.Id)
            .Select(line => new RestaurantOrderLineResponse(
                line.Id,
                line.ProductId,
                line.ProductName,
                line.Quantity,
                line.UnitPrice,
                line.Notes))
            .ToList();

        return new RestaurantOrderResponse(
            order.Id,
            order.TableName,
            order.GuestCount,
            order.WaiterName,
            order.Notes,
            order.Status,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            lines,
            lines.Sum(line => line.Quantity * line.UnitPrice));
    }

    private static string? CleanNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}

/// <summary>Menu product shown by the restaurant ordering app.</summary>
public sealed record MenuItemResponse(int Id, string Name, string Sku, string CategoryName, decimal Price);

/// <summary>Product and quantity submitted for a restaurant order.</summary>
public sealed record CreateRestaurantOrderLineRequest(int ProductId, decimal Quantity, string? Notes);

/// <summary>Payload for creating a table order.</summary>
public sealed record CreateRestaurantOrderRequest(
    string TableName,
    int GuestCount,
    string WaiterName,
    string? Notes,
    List<CreateRestaurantOrderLineRequest> Lines);

/// <summary>Payload for adding products to an existing order.</summary>
public sealed record AddRestaurantOrderLinesRequest(List<CreateRestaurantOrderLineRequest> Lines);

/// <summary>Payload for advancing an order to its next kitchen state.</summary>
public sealed record AdvanceRestaurantOrderStatusRequest(RestaurantOrderStatus Status);

/// <summary>Product captured on a restaurant order.</summary>
public sealed record RestaurantOrderLineResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    string? Notes);

/// <summary>Restaurant order returned by the API.</summary>
public sealed record RestaurantOrderResponse(
    int Id,
    string TableName,
    int GuestCount,
    string WaiterName,
    string? Notes,
    RestaurantOrderStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<RestaurantOrderLineResponse> Lines,
    decimal Total);