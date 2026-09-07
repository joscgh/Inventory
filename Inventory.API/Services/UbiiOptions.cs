namespace Inventory.API.Services
{
    public sealed class UbiiOptions
    {
        public string BaseUrl { get; set; } = "https://botonc.ubiipagos.com";
        public string ClientId { get; set; } = string.Empty;
        public string ClientDomain { get; set; } = string.Empty;
        public string Channel { get; set; } = "BTN-API";
        public string PagoMovilApiKey { get; set; } = string.Empty;
        public string TarjetaDebitoApiKey { get; set; } = string.Empty;
        public string TarjetaDebitoBankCode { get; set; } = string.Empty;
        public string TarjetaDebitoPath { get; set; } = "payment_tdd";
        public bool Enabled { get; set; }
    }

    public sealed class GenericPosOptions
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
        public string ChargePath { get; set; } = "charge";
        public string CancelPath { get; set; } = "cancel";
        public string ProviderCode { get; set; } = "generic-pos";
    }
}