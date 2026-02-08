using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Helpers
{
    public static class CurrencyFormatter
    {
        private static CurrencyService? _currencyService;
        private static SettingsRepository? _settingsRepository;

        public static string Format(decimal amountInBaseCurrency)
        {
            try
            {
                _currencyService ??= new CurrencyService();
                _settingsRepository ??= new SettingsRepository();

                var settings = _settingsRepository.GetSettings();

                // amountInBaseCurrency is stored in USD (or whatever BaseCurrency is set to)
                // Convert and format to the selected display currency
                return _currencyService.FormatAmount(amountInBaseCurrency, settings.SelectedCurrency);
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
                _currencyService ??= new CurrencyService();
                _settingsRepository ??= new SettingsRepository();

                var settings = _settingsRepository.GetSettings();
                var currency = _currencyService.GetCurrency(settings.SelectedCurrency);
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
                _settingsRepository ??= new SettingsRepository();
                var settings = _settingsRepository.GetSettings();
                return settings.SelectedCurrency;
            }
            catch
            {
                return "USD";
            }
        }
    }
}