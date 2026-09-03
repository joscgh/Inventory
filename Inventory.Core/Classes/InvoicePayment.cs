namespace Inventory.Core.Classes
{
    public enum PaymentStatus
    {
        Approved,
        Rejected,
        Pending,
        Cancelled
    }

    public class InvoicePayment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int TerminalId { get; set; }
        public Terminal? Terminal { get; set; }
        public string MethodCode { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = "VES";
        public PaymentStatus Status { get; set; } = PaymentStatus.Approved;
        public string? ProviderCode { get; set; }
        public string? ProviderReference { get; set; }
        public string? AuthorizationCode { get; set; }
        public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    }
}