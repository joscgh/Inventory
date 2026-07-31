using Inventory.Core.Enums;

namespace Inventory.Core.Classes
{
    /// <summary>
    /// Depósito / almacén o tienda que pertenece a una cuenta de cliente.
    /// El campo Type distingue entre ambos, porque comparten los mismos datos fiscales.
    /// </summary>
    public class AccountLocation
    {
        public int Id { get; set; }
        public int CustomerAccountId { get; set; }
        public string Name { get; set; } = string.Empty;
        public LocationType Type { get; set; } = LocationType.Warehouse;
        public string Address { get; set; } = string.Empty;
        public string Rif { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public CustomerAccount? Account { get; set; }
    }
}
