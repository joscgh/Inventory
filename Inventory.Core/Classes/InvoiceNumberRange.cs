using System;

namespace Inventory.Core.Classes
{
    public enum InvoiceRangeStatus
    {
        Active,
        Exhausted,
        Revoked
    }

    /// <summary>
    /// Bloque de números reservado por adelantado para una terminal. Es la pieza que
    /// hace posible facturar sin conexión: la caja recibe el rango mientras hay red y
    /// después puede emitir hasta agotarlo sin consultar al servidor, sin riesgo de
    /// que otra caja use el mismo correlativo.
    ///
    /// El número de control avanza en paralelo al correlativo dentro del rango:
    /// al correlativo FromNumber le corresponde el control ControlFromNumber, al
    /// siguiente el siguiente, y así. Si en tu caso el número de control lo emite una
    /// máquina fiscal, el POS lo manda ya resuelto y el rango sólo administra el
    /// correlativo interno.
    /// </summary>
    public class InvoiceNumberRange
    {
        public int Id { get; set; }

        public int TerminalId { get; set; }
        public Terminal? Terminal { get; set; }

        /// <summary>Cada tipo de documento lleva su propia numeración, incluso en la misma caja.</summary>
        public InvoiceDocumentType DocumentType { get; set; } = InvoiceDocumentType.Factura;

        public string Serie { get; set; } = string.Empty;

        public long FromNumber { get; set; }
        public long ToNumber { get; set; }

        /// <summary>Siguiente correlativo sin usar. Al llegar a ToNumber el rango se agota.</summary>
        public long NextNumber { get; set; }

        /// <summary>Prefijo del número de control, típicamente "00-".</summary>
        public string ControlPrefix { get; set; } = string.Empty;

        /// <summary>Número de control que corresponde a FromNumber.</summary>
        public long ControlFromNumber { get; set; }

        public InvoiceRangeStatus Status { get; set; } = InvoiceRangeStatus.Active;

        public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Los rangos autorizados suelen vencer. Nulo = sin vencimiento.</summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>Número de autorización de la imprenta digital, si aplica.</summary>
        public string? Authorization { get; set; }

        public long Remaining => Status == InvoiceRangeStatus.Active && NextNumber <= ToNumber
            ? ToNumber - NextNumber + 1
            : 0;

        /// <summary>Número de control que le toca a un correlativo de este rango.</summary>
        public string BuildControlNumber(long number)
            => $"{ControlPrefix}{ControlFromNumber + (number - FromNumber)}";
    }
}
