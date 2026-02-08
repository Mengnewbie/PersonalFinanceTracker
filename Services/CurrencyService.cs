using System.Collections.Generic;
using System.Linq;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class CurrencyService
    {
        // Exchange rates as of February 2026 (approximate)
        // All rates are: How many of this currency = 1 USD
        private static readonly List<Currency> _availableCurrencies = new List<Currency>
        {
            new Currency("USD", "$", "US Dollar", 1.0m),
            new Currency("EUR", "€", "Euro", 0.92m),
            new Currency("GBP", "£", "British Pound", 0.79m),
            new Currency("JPY", "¥", "Japanese Yen", 149.50m),
            new Currency("CNY", "¥", "Chinese Yuan", 7.24m),
            new Currency("AUD", "A$", "Australian Dollar", 1.53m),
            new Currency("CAD", "C$", "Canadian Dollar", 1.36m),
            new Currency("CHF", "Fr", "Swiss Franc", 0.88m),
            new Currency("KHR", "៛", "Cambodian Riel", 4050.0m),  
            new Currency("THB", "฿", "Thai Baht", 35.80m),
            new Currency("VND", "₫", "Vietnamese Dong", 24500.0m),
            new Currency("SGD", "S$", "Singapore Dollar", 1.34m),
            new Currency("MYR", "RM", "Malaysian Ringgit", 4.72m),
            new Currency("INR", "₹", "Indian Rupee", 83.12m),
            new Currency("KRW", "₩", "South Korean Won", 1340.0m),
            new Currency("HKD", "HK$", "Hong Kong Dollar", 7.83m),
            new Currency("NZD", "NZ$", "New Zealand Dollar", 1.65m),
            new Currency("SEK", "kr", "Swedish Krona", 10.45m),
            new Currency("NOK", "kr", "Norwegian Krone", 10.72m),
            new Currency("DKK", "kr", "Danish Krone", 6.87m)
        };

        public List<Currency> GetAllCurrencies()
        {
            return _availableCurrencies;
        }

        public Currency GetCurrency(string code)
        {
            return _availableCurrencies.FirstOrDefault(c => c.Code == code)
                   ?? _availableCurrencies.First();
        }

        // Convert from USD to target currency
        public decimal ConvertFromUSD(decimal amountInUSD, string targetCurrencyCode)
        {
            var targetCurrency = GetCurrency(targetCurrencyCode);
            return amountInUSD * targetCurrency.ExchangeRateToUSD;
        }

        // Convert from any currency to USD
        public decimal ConvertToUSD(decimal amount, string sourceCurrencyCode)
        {
            var sourceCurrency = GetCurrency(sourceCurrencyCode);
            if (sourceCurrency.ExchangeRateToUSD == 0) return amount;
            return amount / sourceCurrency.ExchangeRateToUSD;
        }

        // Convert between any two currencies
        public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return amount;

            var amountInUSD = ConvertToUSD(amount, fromCurrency);
            return ConvertFromUSD(amountInUSD, toCurrency);
        }

        public string FormatAmount(decimal amountInUSD, string targetCurrencyCode)
        {
            var convertedAmount = ConvertFromUSD(amountInUSD, targetCurrencyCode);
            var currency = GetCurrency(targetCurrencyCode);

            // Special formatting for currencies without decimals
            if (targetCurrencyCode == "JPY" || targetCurrencyCode == "KRW" ||
                targetCurrencyCode == "VND" || targetCurrencyCode == "KHR")
            {
                return $"{currency.Symbol}{convertedAmount:N0}";
            }

            return $"{currency.Symbol}{convertedAmount:N2}";
        }
    }
}