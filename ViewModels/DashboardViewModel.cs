using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _balance;

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public DashboardViewModel()
        {
            _transactionRepository = new TransactionRepository();
            LoadData();
        }

        private void LoadData()
        {
            TotalIncome = _transactionRepository.GetTotalIncome();
            TotalExpenses = _transactionRepository.GetTotalExpenses();
            Balance = TotalIncome - TotalExpenses;
        }

        public void RefreshData()
        {
            LoadData();
        }
    }
}