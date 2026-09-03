namespace Inventory.Core.Services
{
    public interface IPaymentProvider
    {
        string Code { get; }
        Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default);
        Task<PaymentProviderResult> CancelAsync(string reference, CancellationToken cancellationToken = default);
    }

    public sealed record PaymentChargeRequest(
        decimal Amount,
        string CurrencyCode,
        string MethodCode,
        int TerminalId,
        string? ProviderData = null);

    public sealed record PaymentProviderResult(
        bool Approved,
        string? Reference = null,
        string? AuthorizationCode = null,
        string? ErrorMessage = null);
}