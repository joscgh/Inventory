namespace Inventory.Core.Classes
{
    /// <summary>
    /// Logo del cliente. Vive en su propia tabla (1:1 con la cuenta) para que
    /// listar cuentas no tenga que traer los bytes de todas las imágenes.
    /// </summary>
    public class AccountLogo
    {
        public int CustomerAccountId { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public CustomerAccount? Account { get; set; }
    }
}
