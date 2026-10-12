using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inventory.Core.Services;
using Microsoft.Extensions.Options;

namespace Inventory.API.Services
{
    public sealed class UbiiPaymentProvider : IPaymentProvider
    {
        private readonly UbiiApiClient _client;
        private readonly UbiiOptions _options;
        private readonly GenericPosOptions _genericOptions;
        private readonly StripeOptions _stripeOptions;
        private readonly GenericPosPaymentProvider _genericProvider;
        private readonly StripePaymentProvider _stripeProvider;
        private readonly ManualPaymentProvider _manualProvider;

        public UbiiPaymentProvider(
            UbiiApiClient client,
            IOptions<UbiiOptions> options,
            IOptions<GenericPosOptions> genericOptions,
            IOptions<StripeOptions> stripeOptions,
            GenericPosPaymentProvider genericProvider,
            StripePaymentProvider stripeProvider,
            ManualPaymentProvider manualProvider)
        {
            _client = client;
            _options = options.Value;
            _genericOptions = genericOptions.Value;
            _stripeOptions = stripeOptions.Value;
            _genericProvider = genericProvider;
            _stripeProvider = stripeProvider;
            _manualProvider = manualProvider;
        }

        public string Code => "ubii";

        public async Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (IsMobile(request.MethodCode))
                {
                    return await _client.ValidateMobilePaymentAsync(request, cancellationToken);
                }

                if (IsDebitCard(request.MethodCode))
                {
                    return await _client.ProcessDebitCardAsync(request, cancellationToken);
                }

                if (PaymentMethodClassifier.IsInternationalCard(request.MethodCode, request.CurrencyCode))
                {
                    return _stripeOptions.Enabled
                        ? await _stripeProvider.ChargeAsync(request, cancellationToken)
                        : await _manualProvider.ChargeAsync(request, cancellationToken);
                }

                return _genericOptions.Enabled
                    ? await _genericProvider.ChargeAsync(request, cancellationToken)
                    : await _manualProvider.ChargeAsync(request, cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                return new PaymentProviderResult(false, ErrorMessage: exception.Message);
            }
        }

        public Task<PaymentProviderResult> CancelAsync(string reference, CancellationToken cancellationToken = default)
            => Task.FromResult(new PaymentProviderResult(false, reference, ErrorMessage: "Ubii no tiene reverso implementado para este flujo."));

        private static bool IsMobile(string methodCode) => PaymentMethodClassifier.IsMobile(methodCode);

        private static bool IsDebitCard(string methodCode) => PaymentMethodClassifier.IsDebitCard(methodCode);
    }
}