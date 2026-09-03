using Inventory.Core.Services;

namespace Inventory.API.Services
{
    public sealed class ManualPaymentProvider : IPaymentProvider
    {
        public string Code => "manual";

        public Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaymentProviderResult(true));

        public Task<PaymentProviderResult> CancelAsync(string reference, CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaymentProviderResult(true, reference));
    }
}