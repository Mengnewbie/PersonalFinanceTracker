using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class BudgetItemViewModel : BaseViewModel
    {
        private readonly CurrencyService _currencyService;
        private string _category;
        private decimal _budgetAmount;
        private string _budgetCurrency;
        private decimal _spent;
        private decimal _remaining;
        private double _progressPercentage;
        private string _statusColor;
        private string _statusText;
        private string _icon;
        private int _budgetId;

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
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

        public string BudgetCurrency
        {
            get => _budgetCurrency;
            set
            {
                if (SetProperty(ref _budgetCurrency, value))
                {
                    OnPropertyChanged(nameof(BudgetAmountFormatted));
                    OnPropertyChanged(nameof(SpentFormatted));
                    OnPropertyChanged(nameof(RemainingFormatted));
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
                {
                    OnPropertyChanged(nameof(RemainingFormatted));
                }
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

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public int BudgetId
        {
            get => _budgetId;
            set => SetProperty(ref _budgetId, value);
        }

        // Formatted properties
        public string BudgetAmountFormatted => FormatCurrency(BudgetAmount);
        public string SpentFormatted => FormatCurrency(Spent);
        public string RemainingFormatted => FormatCurrency(Remaining);

        public BudgetItemViewModel(CurrencyService currencyService)
        {
            _currencyService = currencyService;
            _category = string.Empty;
            _budgetCurrency = "USD";
            _statusColor = "#27AE60";
            _statusText = "On Track";
            _icon = "📊";
        }

        private string FormatCurrency(decimal amount)
        {
            if (_currencyService == null) return $"${amount:N2}";

            var currency = _currencyService.GetCurrency(BudgetCurrency);

            // Special formatting for currencies without decimals
            if (BudgetCurrency == "JPY" || BudgetCurrency == "KRW" ||
                BudgetCurrency == "VND" || BudgetCurrency == "KHR")
            {
                return $"{currency.Symbol}{amount:N0}";
            }

            return $"{currency.Symbol}{amount:N2}";
        }

        private void CalculateProgress()
        {
            Remaining = BudgetAmount - Spent;

            if (BudgetAmount > 0)
            {
                ProgressPercentage = (double)(Spent / BudgetAmount * 100);

                // Determine status and color
                if (ProgressPercentage >= 100)
                {
                    StatusColor = "#E74C3C"; // Red - Over budget
                    StatusText = "Over Budget!";
                }
                else if (ProgressPercentage >= 80)
                {
                    StatusColor = "#F39C12"; // Orange - Warning
                    StatusText = "Warning";
                }
                else
                {
                    StatusColor = "#27AE60"; // Green - On track
                    StatusText = "On Track";
                }
            }
            else
            {
                ProgressPercentage = 0;
            }
        }

        public void RefreshFormatting()
        {
            OnPropertyChanged(nameof(BudgetAmountFormatted));
            OnPropertyChanged(nameof(SpentFormatted));
            OnPropertyChanged(nameof(RemainingFormatted));
        }
    }
}