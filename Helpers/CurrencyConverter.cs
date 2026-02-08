using System;
using System.Globalization;
using System.Windows.Data;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Helpers
{
    public class CurrencyConverter : IValueConverter
    {
        private static CurrencyService? _currencyService;
        private static SettingsRepository? _settingsRepository;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                try
                {
                    _currencyService ??= new CurrencyService();
                    _settingsRepository ??= new SettingsRepository();

                    var settings = _settingsRepository.GetSettings();
                    return _currencyService.FormatAmount(amount, settings.SelectedCurrency);
                }
                catch
                {
                    // Fallback to default USD formatting if something fails
                    return $"${amount:N2}";
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}