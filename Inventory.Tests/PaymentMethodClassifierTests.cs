using Inventory.Core.Services;

namespace Inventory.Tests;

public class PaymentMethodClassifierTests
{
    [Theory]
    [InlineData("stripe_card", "USD")]
    [InlineData("international_card", "USD")]
    [InlineData("card", "USD")]
    public void ShouldDetectInternationalCard(string methodCode, string currencyCode)
    {
        Assert.True(PaymentMethodClassifier.IsInternationalCard(methodCode, currencyCode));
    }

    [Theory]
    [InlineData("card", "VES")]
    [InlineData("tdd", "VES")]
    [InlineData("mobile", "VES")]
    public void ShouldNotTreatLocalPaymentsAsInternational(string methodCode, string currencyCode)
    {
        Assert.False(PaymentMethodClassifier.IsInternationalCard(methodCode, currencyCode));
    }
}
