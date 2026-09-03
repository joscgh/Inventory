using System;
using System.Collections.Generic;

namespace Inventory.Core.Classes
{
    /// <summary>
    /// Caja / punto de venta que emite facturas. Puede ser un POS Android o Windows
    /// instalado en la tienda, o la propia PWA usada desde un escritorio.
    ///
    /// Existe por la numeración: cada terminal tiene su propia serie y sus propios
    /// rangos de números reservados, que es lo que permite facturar sin conexión sin
    /// que dos cajas emitan el mismo correlativo.
    /// </summary>
    public class Terminal
    {
        public int Id { get; set; }

        public int CustomerAccountId { get; set; }
        public CustomerAccount? Account { get; set; }

        /// <summary>Tienda donde está instalada la caja. Es la que aparece como emisor en la factura.</summary>
        public int? StoreId { get; set; }
        public AccountLocation? Store { get; set; }

        /// <summary>Identificador corto de la caja: "A", "CAJA1".</summary>
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        /// <summary>Serie fiscal con la que numera esta caja. Debe ser distinta entre terminales de la misma cuenta.</summary>
        public string Serie { get; set; } = string.Empty;

        /// <summary>
        /// Identificador del dispositivo donde quedó instalada la app (para detectar
        /// que un rango se está usando desde un equipo distinto al esperado).
        /// </summary>
        public string? DeviceIdentifier { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public List<InvoiceNumberRange> Ranges { get; set; } = new();
        public List<TerminalPaymentMethod> PaymentMethods { get; set; } = new();
    }
}
