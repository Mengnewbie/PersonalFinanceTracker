using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Helpers
{
    public static class CurrencyFormatter
    {
        // FIX: Removed static cached instances (_currencyService, _settingsRepository)
        // They caused stale data after settings changes because ??= never refreshes

        public static string Format(decimal amountInBaseCurrency)
        {
            try
            {
                var currencyService = new CurrencyService();
                var settingsRepository = new SettingsRepository();

                var settings = settingsRepository.GetSettings();
                return currencyService.FormatAmount(amountInBaseCurrency, settings.SelectedCurrency);
            }
            catch
            {
                return $"${amountInBaseCurrency:N2}";
            }
        }

        public static string GetCurrentCurrencySymbol()
        {
            try
            {
                var currencyService = new CurrencyService();
                var settingsRepository = new SettingsRepository();

                var settings = settingsRepository.GetSettings();
                var currency = currencyService.GetCurrency(settings.SelectedCurrency);
                return currency.Symbol;
            }
            catch
            {
                return "$";
            }
        }

        public static string GetCurrentCurrencyCode()
        {
            try
            {
                var settingsRepository = new SettingsRepository();
                var settings = settingsRepository.GetSettings();
                return settings.SelectedCurrency;
            }
            catch
            {
                return "USD";
            }
        }
    }
}