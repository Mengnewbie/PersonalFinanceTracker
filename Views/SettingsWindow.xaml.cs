using System.Windows;
using System.Windows.Controls;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly CurrencyService _currencyService;
        private readonly SettingsRepository _settingsRepository;
        public bool IsSaved { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            _currencyService = new CurrencyService();
            _settingsRepository = new SettingsRepository();

            LoadCurrencies();
        }

        private void LoadCurrencies()
        {
            var currencies = _currencyService.GetAllCurrencies();
            CurrencyComboBox.ItemsSource = currencies;

            // Load current setting
            var settings = _settingsRepository.GetSettings();
            var selectedCurrency = _currencyService.GetCurrency(settings.SelectedCurrency);
            CurrencyComboBox.SelectedItem = selectedCurrency;
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrencyComboBox.SelectedItem is Currency currency)
            {
                // Show conversion example
                decimal usdAmount = 1234.56m;
                decimal convertedAmount = _currencyService.ConvertFromUSD(usdAmount, currency.Code);

                PreviewText.Text = _currencyService.FormatAmount(usdAmount, currency.Code);

                // Show Riel conversion from $100
                decimal usd100 = 100m;
                decimal convertedRiel = _currencyService.ConvertFromUSD(usd100, currency.Code);
                PreviewRiel.Text = $"{_currencyService.FormatAmount(usd100, currency.Code)} (from $100 USD)";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrencyComboBox.SelectedItem is Currency currency)
            {
                _settingsRepository.UpdateCurrency(currency.Code);
                IsSaved = true;

                var rateInfo = currency.Code != "USD"
                    ? $"\n\nExchange Rate: 1 USD = {currency.ExchangeRateToUSD:N2} {currency.Code}"
                    : "";

                MessageBox.Show(
                    $"Currency changed to {currency.Name}!{rateInfo}\n\nAll amounts will be converted and displayed in {currency.Code}.\n\nThe app will now refresh.",
                    "Settings Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            Close();
        }
    }
}