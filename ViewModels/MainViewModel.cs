using System.Windows.Input;
using PersonalFinanceTracker.Commands;

namespace PersonalFinanceTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _title;
        private BaseViewModel _currentViewModel;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // Navigation Commands
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToTransactionsCommand { get; }
        public ICommand NavigateToCategoriesCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand NavigateToBudgetCommand { get; }

        public MainViewModel()
        {
            _title = "Personal Finance Tracker";
            _currentViewModel = new DashboardViewModel();

            // Initialize commands
            NavigateToDashboardCommand = new RelayCommand(ExecuteNavigateToDashboard);
            NavigateToTransactionsCommand = new RelayCommand(ExecuteNavigateToTransactions);
            NavigateToCategoriesCommand = new RelayCommand(ExecuteNavigateToCategories);
            NavigateToReportsCommand = new RelayCommand(ExecuteNavigateToReports);
            NavigateToBudgetCommand = new RelayCommand(ExecuteNavigateToBudget);
        }

        private void ExecuteNavigateToDashboard(object? parameter)
        {
            CurrentViewModel = new DashboardViewModel();
        }

        private void ExecuteNavigateToTransactions(object? parameter)
        {
            CurrentViewModel = new TransactionsViewModel();
        }

        private void ExecuteNavigateToCategories(object? parameter)
        {
            CurrentViewModel = new CategoriesViewModel();
        }

        private void ExecuteNavigateToReports(object? parameter)
        {
            CurrentViewModel = new ReportsViewModel();
        }

        private void ExecuteNavigateToBudget(object? parameter)
        {
            CurrentViewModel = new BudgetViewModel();
        }
    }
}