using System;
using System.Globalization;
using System.Windows.Data;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Helpers
{
    public class CurrencyConverter : IValueConverter
    {
        // FIX: Create fresh instances each time to avoid stale cached settings
        // The previous version used static ??= which meant settings changes
        // were never reflected until app restart

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                try
                {
                    var currencyService = new CurrencyService();
                    var settingsRepository = new SettingsRepository();

                    var settings = settingsRepository.GetSettings();
                    return currencyService.FormatAmount(amount, settings.SelectedCurrency);
                }
                catch
                {
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