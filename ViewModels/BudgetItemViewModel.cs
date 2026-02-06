namespace PersonalFinanceTracker.ViewModels
{
    public class BudgetItemViewModel : BaseViewModel
    {
        private string _category;
        private decimal _budgetAmount;
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
            set => SetProperty(ref _budgetAmount, value);
        }

        public decimal Spent
        {
            get => _spent;
            set
            {
                SetProperty(ref _spent, value);
                CalculateProgress();
            }
        }

        public decimal Remaining
        {
            get => _remaining;
            set => SetProperty(ref _remaining, value);
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

        public BudgetItemViewModel()
        {
            _category = string.Empty;
            _statusColor = "#27AE60";
            _statusText = "On Track";
            _icon = "📊";
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
    }
}