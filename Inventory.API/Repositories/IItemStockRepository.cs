using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface IItemStockRepository
    {
        Task<IEnumerable<ItemStock>> GetByItemAsync(int itemId);

        /// <summary>Ubicaciones donde el artículo tiene existencias distintas de cero.</summary>
        Task<List<int>> GetLocationIdsWithStockAsync(int itemId);

        /// <summary>Fija la cantidad absoluta en una ubicación y devuelve el nuevo total del artículo.</summary>
        Task<double> SetQuantityAsync(int itemId, int locationId, double quantity);

        /// <summary>
        /// Suma un delta a una ubicación (el resultado nunca baja de cero) y devuelve
        /// la cantidad previa y la nueva en esa ubicación, más el nuevo total del artículo.
        /// </summary>
        Task<(double PreviousAtLocation, double NewAtLocation, double NewTotal)> AdjustAsync(
            int itemId, int locationId, double delta);
    }
}
