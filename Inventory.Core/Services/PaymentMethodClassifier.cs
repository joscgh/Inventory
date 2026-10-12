namespace Inventory.Core.Services;

public static class PaymentMethodClassifier
{
    public static bool IsMobile(string methodCode) =>
        methodCode.Equals("mobile", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("p2c", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("p2cr", StringComparison.OrdinalIgnoreCase);

    public static bool IsDebitCard(string methodCode) =>
        methodCode.Equals("card", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("tdd", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("debit_card", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("debit", StringComparison.OrdinalIgnoreCase);

    public static bool IsInternationalCard(string methodCode, string currencyCode) =>
        IsCardLike(methodCode)
        && !string.Equals(currencyCode, "VES", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(currencyCode, "BS", StringComparison.OrdinalIgnoreCase);

    public static bool IsCardLike(string methodCode) =>
        methodCode.Equals("card", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("stripe_card", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("international_card", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("stripe", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("visa", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("mastercard", StringComparison.OrdinalIgnoreCase)
        || methodCode.Equals("amex", StringComparison.OrdinalIgnoreCase);
}
