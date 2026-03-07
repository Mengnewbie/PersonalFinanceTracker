using System.Collections.Generic;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class BudgetItemViewModel : BaseViewModel
    {
        private static readonly HashSet<string> NoDecimalCurrencies = new() { "JPY", "KRW", "VND", "KHR" };

        private readonly CurrencyService _currencyService;

        private int _budgetId;
        private string _category;
        private string _budgetCurrency;
        private string _icon;
        private decimal _budgetAmount;
        private decimal _spent;
        private decimal _remaining;
        private double _progressPercentage;
        private string _statusColor;
        private string _statusText;

        #region Properties

        public int BudgetId
        {
            get => _budgetId;
            set => SetProperty(ref _budgetId, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string BudgetCurrency
        {
            get => _budgetCurrency;
            set
            {
                if (SetProperty(ref _budgetCurrency, value))
                    NotifyFormattedProperties();
            }
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public decimal BudgetAmount
        {
            get => _budgetAmount;
            set
            {
                if (SetProperty(ref _budgetAmount, value))
                {
                    CalculateProgress();
                    OnPropertyChanged(nameof(BudgetAmountFormatted));
                }
            }
        }

        public decimal Spent
        {
            get => _spent;
            set
            {
                if (SetProperty(ref _spent, value))
                {
                    CalculateProgress();
                    OnPropertyChanged(nameof(SpentFormatted));
                }
            }
        }

        public decimal Remaining
        {
            get => _remaining;
            set
            {
                if (SetProperty(ref _remaining, value))
                    OnPropertyChanged(nameof(RemainingFormatted));
            }
        }

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        #endregion

        #region Formatted Properties

        public string BudgetAmountFormatted => FormatCurrency(BudgetAmount);
        public string SpentFormatted => FormatCurrency(Spent);
        public string RemainingFormatted => FormatCurrency(Remaining);

        #endregion

        public BudgetItemViewModel(CurrencyService currencyService)
        {
            _currencyService = currencyService;
            _category = string.Empty;
            _budgetCurrency = "USD";
            _icon = "📊";
            _statusColor = "#27AE60";
            _statusText = "On Track";
        }

        private string FormatCurrency(decimal amount)
        {
            if (_currencyService == null) return $"${amount:N2}";

            var currency = _currencyService.GetCurrency(BudgetCurrency);
            var format = NoDecimalCurrencies.Contains(BudgetCurrency) ? "N0" : "N2";

            return $"{currency.Symbol}{amount.ToString(format)}";
        }

        private void CalculateProgress()
        {
            Remaining = BudgetAmount - Spent;

            if (BudgetAmount <= 0)
            {
                ProgressPercentage = 0;
                return;
            }

            ProgressPercentage = (double)(Spent / BudgetAmount * 100);

            if (ProgressPercentage >= 100)
            {
                StatusColor = "#E74C3C";
                StatusText = "Over Budget!";
            }
            else if (ProgressPercentage >= 80)
            {
                StatusColor = "#F39C12";
                StatusText = "Warning";
            }
            else
            {
                StatusColor = "#27AE60";
                StatusText = "On Track";
            }
        }

        public void RefreshFormatting() => NotifyFormattedProperties();

        private void NotifyFormattedProperties()
        {
            OnPropertyChanged(nameof(BudgetAmountFormatted));
            OnPropertyChanged(nameof(SpentFormatted));
            OnPropertyChanged(nameof(RemainingFormatted));
        }
    }
}