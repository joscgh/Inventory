using System.Text.RegularExpressions;

namespace Inventory.Core.Services
{
    public static class FiscalIdentifierValidator
    {
        private static readonly Regex RifPattern = new(
            @"^[VEJPGC]-?\d{8,9}-?\d$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool IsValidRif(string? value)
            => !string.IsNullOrWhiteSpace(value) && RifPattern.IsMatch(value.Trim());

        public static string NormalizeRif(string value)
            => value.Trim().ToUpperInvariant().Replace(" ", string.Empty);
    }
}