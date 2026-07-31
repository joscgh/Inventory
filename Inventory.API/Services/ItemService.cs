using Inventory.Core.Classes;
using Inventory.API.Repositories;

namespace Inventory.API.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly Repositories.IAdjustmentRepository _adjustmentRepository;
        private readonly IItemStockRepository _stockRepository;
        private readonly IAccountLocationRepository _locationRepository;

        public ItemService(
            IItemRepository repository,
            Repositories.IAdjustmentRepository adjustmentRepository,
            IItemStockRepository stockRepository,
            IAccountLocationRepository locationRepository)
        {
            _repository = repository;
            _adjustmentRepository = adjustmentRepository;
            _stockRepository = stockRepository;
            _locationRepository = locationRepository;
        }

        public async Task<IEnumerable<ItemUniversal>> ListItemAsync() =>
            await _repository.GetAllAsync();

        public async Task<ItemUniversal?> FindByIdAsync(string id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterItemAsync(ItemUniversal item)
        {
            if (item.Price < 0 || item.Stock < 0 || item.TaxRate < 0 || item.TaxRate > 100
                || item.Cost < 0 || item.ProfitMargin < 0) return false;

            ApplyMarginPricing(item);

            var existingItem = await _repository.GetByIdAsync(item.SKU);
            if (existingItem != null) return false;

            // Las existencias iniciales vienen por ubicación; el total es su suma.
            item.StockByLocation.RemoveAll(s => s.LocationId == 0);
            if (item.StockByLocation.Any())
            {
                item.Stock = Math.Round(item.StockByLocation.Sum(s => s.Quantity), 2);
            }

            await _repository.AddAsync(item);
            return true;
        }

        public async Task<bool> ModifyItemAsync(ItemUniversal item)
        {
            if (item.Price < 0 || item.TaxRate < 0 || item.TaxRate > 100
                || item.Cost < 0 || item.ProfitMargin < 0) return false;

            ApplyMarginPricing(item);

            var existingItem = await _repository.GetByIdAsync(item.SKU);
            if (existingItem == null) return false;

            // Editar los datos del artículo nunca altera las existencias: el total
            // se mantiene y las cantidades por ubicación tienen su propio endpoint.
            item.Stock = existingItem.Stock;
            item.StockByLocation.Clear();

            await _repository.UpdateAsync(item);
            return true;
        }

        // Precio de venta = Costo x (1 + Margen/100). El servidor es la fuente de
        // verdad: si hay un costo definido, el precio se deriva del margen.
        private static void ApplyMarginPricing(ItemUniversal item)
        {
            if (item.Cost > 0)
            {
                item.Price = Math.Round(item.Cost * (1 + item.ProfitMargin / 100m), 2);
            }
        }

        public async Task<(bool Success, string? Error)> AdjustStockAsync(
            string sku, double delta, string reason, string referenceType, int? locationId)
        {
            var existingItem = await _repository.GetByIdAsync(sku);
            if (existingItem == null) return (false, $"No existe el artículo con SKU {sku}.");

            var (resolvedLocation, locationError) = await ResolveLocationAsync(existingItem.Id, locationId);
            if (resolvedLocation == null) return (false, locationError);

            var previousTotal = existingItem.Stock;
            var (_, _, newTotal) = await _stockRepository.AdjustAsync(existingItem.Id, resolvedLocation.Value, delta);

            await _adjustmentRepository.AddAsync(new Inventory.Core.Classes.InventoryAdjustment
            {
                ItemId = existingItem.Id,
                SKU = existingItem.SKU,
                LocationId = resolvedLocation.Value,
                Change = newTotal - previousTotal,
                PreviousStock = previousTotal,
                NewStock = newTotal,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Stock adjustment" : reason,
                ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? "Manual" : referenceType,
                Timestamp = DateTime.UtcNow
            });

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> SetStockAtLocationAsync(
            string sku, int locationId, double quantity, string reason)
        {
            if (quantity < 0) return (false, "La cantidad no puede ser negativa.");

            var existingItem = await _repository.GetByIdAsync(sku);
            if (existingItem == null) return (false, $"No existe el artículo con SKU {sku}.");

            if (await _locationRepository.GetByIdAsync(locationId) == null)
                return (false, "No existe el depósito o tienda indicado.");

            var previousTotal = existingItem.Stock;
            var previousAtLocation = existingItem.StockAt(locationId);
            var newTotal = await _stockRepository.SetQuantityAsync(existingItem.Id, locationId, quantity);

            // Sólo dejamos rastro si la cantidad de esa ubicación cambió de verdad.
            if (Math.Abs(previousAtLocation - Math.Round(quantity, 2)) > 0.0001)
            {
                await _adjustmentRepository.AddAsync(new Inventory.Core.Classes.InventoryAdjustment
                {
                    ItemId = existingItem.Id,
                    SKU = existingItem.SKU,
                    LocationId = locationId,
                    Change = newTotal - previousTotal,
                    PreviousStock = previousTotal,
                    NewStock = newTotal,
                    Reason = string.IsNullOrWhiteSpace(reason) ? "Existencias fijadas por ubicación" : reason,
                    ReferenceType = "Ubicacion",
                    Timestamp = DateTime.UtcNow
                });
            }

            return (true, null);
        }

        public async Task<IEnumerable<Inventory.Core.Classes.ItemStock>> ListStockByLocationAsync(string sku)
        {
            var item = await _repository.GetByIdAsync(sku);
            if (item == null) return Enumerable.Empty<Inventory.Core.Classes.ItemStock>();

            return await _stockRepository.GetByItemAsync(item.Id);
        }

        /// <summary>
        /// Valida la ubicación pedida o, si no se indicó ninguna, deduce la única
        /// donde el artículo tiene existencias. Con varias candidatas exige elegir.
        /// </summary>
        private async Task<(int? LocationId, string? Error)> ResolveLocationAsync(int itemId, int? locationId)
        {
            if (locationId.HasValue)
            {
                return await _locationRepository.GetByIdAsync(locationId.Value) == null
                    ? (null, "No existe el depósito o tienda indicado.")
                    : (locationId.Value, null);
            }

            var withStock = await _stockRepository.GetLocationIdsWithStockAsync(itemId);
            if (withStock.Count == 1) return (withStock[0], null);

            return withStock.Count == 0
                ? (null, "El artículo no tiene existencias en ningún depósito. Indica el depósito o tienda del movimiento.")
                : (null, "El artículo tiene existencias en varios depósitos. Indica de cuál se hace el movimiento.");
        }

        public async Task<bool> RemoveItemAsync(string id)
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Sum(i => i.CalculateInventoryValue());
        }
    }
}
