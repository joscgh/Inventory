using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/restaurant-tables")]
public sealed class RestaurantTablesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RestaurantTablesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RestaurantTableResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var tables = await _context.RestaurantTables
            .AsNoTracking()
            .OrderBy(table => table.Id)
            .Select(table => ToResponse(table))
            .ToListAsync(cancellationToken);

        return Ok(tables);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RestaurantTableResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var table = await _context.RestaurantTables
            .AsNoTracking()
            .FirstOrDefaultAsync(value => value.Id == id, cancellationToken);

        return table is null ? NotFound() : Ok(ToResponse(table));
    }

    [HttpPost]
    public async Task<ActionResult<RestaurantTableResponse>> Create(
        CreateRestaurantTableRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80 || request.Capacity is < 1 or > 30)
            return BadRequest("Indica un nombre de hasta 80 caracteres y una capacidad entre 1 y 30.");

        if (await NameExists(name, null, cancellationToken))
            return Conflict("Ya existe una mesa con ese nombre.");

        var table = new RestaurantTable { Name = name, Capacity = request.Capacity };
        _context.RestaurantTables.Add(table);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = table.Id }, ToResponse(table));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RestaurantTableResponse>> Update(
        int id,
        UpdateRestaurantTableRequest request,
        CancellationToken cancellationToken)
    {
        var table = await _context.RestaurantTables.FirstOrDefaultAsync(value => value.Id == id, cancellationToken);
        if (table is null) return NotFound();

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80 || request.Capacity is < 1 or > 30)
            return BadRequest("Indica un nombre de hasta 80 caracteres y una capacidad entre 1 y 30.");

        if (await NameExists(name, id, cancellationToken))
            return Conflict("Ya existe una mesa con ese nombre.");

        var hasOpenOrder = await _context.RestaurantOrders.AnyAsync(
            order => order.TableName == table.Name && order.Status != RestaurantOrderStatus.Closed,
            cancellationToken);
        if (hasOpenOrder && (!table.IsActive || !request.IsActive || !string.Equals(table.Name, name, StringComparison.Ordinal)))
            return Conflict("No se puede renombrar ni desactivar una mesa con una comanda abierta.");

        table.Name = name;
        table.Capacity = request.Capacity;
        table.IsActive = request.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(table));
    }

    private Task<bool> NameExists(string name, int? exceptId, CancellationToken cancellationToken) =>
        _context.RestaurantTables.AnyAsync(
            table => table.Id != exceptId && table.Name.ToLower() == name.ToLower(),
            cancellationToken);

    private static RestaurantTableResponse ToResponse(RestaurantTable table) =>
        new(table.Id, table.Name, table.Capacity, table.IsActive);
}

/// <summary>Payload for creating a restaurant table.</summary>
public sealed record CreateRestaurantTableRequest(string Name, int Capacity);

/// <summary>Payload for updating a restaurant table.</summary>
public sealed record UpdateRestaurantTableRequest(string Name, int Capacity, bool IsActive);

/// <summary>Restaurant table returned by the API.</summary>
public sealed record RestaurantTableResponse(int Id, string Name, int Capacity, bool IsActive);