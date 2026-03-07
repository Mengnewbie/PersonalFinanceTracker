using System.Windows.Input;
using PersonalFinanceTracker.Commands;

namespace PersonalFinanceTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _title;
        private BaseViewModel _currentViewModel;
        private string _activePage;

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

        /// <summary>
        /// Tracks which page is active so the sidebar can highlight it.
        /// Values: "Dashboard", "Transactions", "Categories", "Reports", "Budget"
        /// </summary>
        public string ActivePage
        {
            get => _activePage;
            set => SetProperty(ref _activePage, value);
        }

        // Navigation Commands
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToTransactionsCommand { get; }
        public ICommand NavigateToCategoriesCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand NavigateToBudgetCommand { get; }

        public MainViewModel()
        {
            _title = "FinTrack — Personal Finance Tracker";
            _activePage = "Dashboard";
            _currentViewModel = new DashboardViewModel();

            NavigateToDashboardCommand = new RelayCommand(_ => NavigateTo<DashboardViewModel>("Dashboard"));
            NavigateToTransactionsCommand = new RelayCommand(_ => NavigateTo<TransactionsViewModel>("Transactions"));
            NavigateToCategoriesCommand = new RelayCommand(_ => NavigateTo<CategoriesViewModel>("Categories"));
            NavigateToReportsCommand = new RelayCommand(_ => NavigateTo<ReportsViewModel>("Reports"));
            NavigateToBudgetCommand = new RelayCommand(_ => NavigateTo<BudgetViewModel>("Budget"));
        }

        private void NavigateTo<T>(string pageName) where T : BaseViewModel, new()
        {
            // Skip if already on the same page type
            if (CurrentViewModel is T) return;

            ActivePage = pageName;
            CurrentViewModel = new T();
        }
    }
}