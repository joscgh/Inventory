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
        private readonly GenericPosPaymentProvider _genericProvider;
        private readonly ManualPaymentProvider _manualProvider;

        public UbiiPaymentProvider(
            UbiiApiClient client,
            IOptions<UbiiOptions> options,
            IOptions<GenericPosOptions> genericOptions,
            GenericPosPaymentProvider genericProvider,
            ManualPaymentProvider manualProvider)
        {
            _client = client;
            _options = options.Value;
            _genericOptions = genericOptions.Value;
            _genericProvider = genericProvider;
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

        private static bool IsMobile(string methodCode) =>
            methodCode.Equals("mobile", StringComparison.OrdinalIgnoreCase)
            || methodCode.Equals("p2c", StringComparison.OrdinalIgnoreCase)
            || methodCode.Equals("p2cr", StringComparison.OrdinalIgnoreCase);

        private static bool IsDebitCard(string methodCode) =>
            methodCode.Equals("card", StringComparison.OrdinalIgnoreCase)
            || methodCode.Equals("tdd", StringComparison.OrdinalIgnoreCase);
    }
}