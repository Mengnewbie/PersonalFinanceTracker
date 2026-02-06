using System.Text.RegularExpressions;

namespace PersonalFinanceTracker.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidDecimal(string input, out decimal result)
        {
            return decimal.TryParse(input, out result) && result > 0;
        }

        public static bool IsValidAmount(decimal amount)
        {
            return amount > 0 && amount <= 999999999;
        }

        public static bool IsNotEmpty(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        public static bool IsValidColorHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return false;

            // Check if it matches hex color pattern
            var hexPattern = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
            return Regex.IsMatch(hex, hexPattern);
        }

        public static string SanitizeInput(string input)
        {
            return input?.Trim() ?? string.Empty;
        }
    }
}
