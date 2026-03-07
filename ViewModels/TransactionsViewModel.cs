using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly CurrencyService _currencyService;

        private ObservableCollection<Transaction> _transactions;
        private ObservableCollection<Transaction> _filteredTransactions;
        private Transaction? _selectedTransaction;

        // Filters
        private string _searchText;
        private string _selectedTypeFilter;
        private string _selectedCategoryFilter;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _selectedSortOption;

        private ObservableCollection<string> _typeFilters;
        private ObservableCollection<string> _categoryFilters;
        private ObservableCollection<string> _sortOptions;

        // Summary
        private int _displayedCount;
        private int _totalCount;
        private decimal _displayedIncome;
        private decimal _displayedExpenses;
        private decimal _displayedBalance;

        #region Properties

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        public ObservableCollection<Transaction> FilteredTransactions
        {
            get => _filteredTransactions;
            set => SetProperty(ref _filteredTransactions, value);
        }

        public Transaction? SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) ApplyFilters(); }
        }

        public string SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set { if (SetProperty(ref _selectedTypeFilter, value)) ApplyFilters(); }
        }

        public string SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set { if (SetProperty(ref _selectedCategoryFilter, value)) ApplyFilters(); }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set { if (SetProperty(ref _startDate, value)) ApplyFilters(); }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set { if (SetProperty(ref _endDate, value)) ApplyFilters(); }
        }

        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set { if (SetProperty(ref _selectedSortOption, value)) ApplyFilters(); }
        }

        public ObservableCollection<string> TypeFilters
        {
            get => _typeFilters;
            set => SetProperty(ref _typeFilters, value);
        }

        public ObservableCollection<string> CategoryFilters
        {
            get => _categoryFilters;
            set => SetProperty(ref _categoryFilters, value);
        }

        public ObservableCollection<string> SortOptions
        {
            get => _sortOptions;
            set => SetProperty(ref _sortOptions, value);
        }

        public int DisplayedCount
        {
            get => _displayedCount;
            set => SetProperty(ref _displayedCount, value);
        }

        public decimal DisplayedIncome
        {
            get => _displayedIncome;
            set => SetProperty(ref _displayedIncome, value);
        }

        public decimal DisplayedExpenses
        {
            get => _displayedExpenses;
            set => SetProperty(ref _displayedExpenses, value);
        }

        public decimal DisplayedBalance
        {
            get => _displayedBalance;
            set => SetProperty(ref _displayedBalance, value);
        }

        // Formatted display properties
        public string DisplayedIncomeFormatted => CurrencyFormatter.Format(DisplayedIncome);
        public string DisplayedExpensesFormatted => CurrencyFormatter.Format(DisplayedExpenses);
        public string DisplayedBalanceFormatted => CurrencyFormatter.Format(DisplayedBalance);

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand RowDoubleClickCommand { get; }

        #endregion

        public TransactionsViewModel()
        {
            _transactionRepository = new TransactionRepository();
            _categoryRepository = new CategoryRepository();
            _currencyService = new CurrencyService();

            _transactions = new ObservableCollection<Transaction>();
            _filteredTransactions = new ObservableCollection<Transaction>();

            _searchText = string.Empty;
            _selectedTypeFilter = "All";
            _selectedCategoryFilter = "All";
            _selectedSortOption = "Date (Newest First)";

            _typeFilters = new ObservableCollection<string> { "All", "Income", "Expense" };
            _categoryFilters = new ObservableCollection<string> { "All" };
            _sortOptions = new ObservableCollection<string>
            {
                "Date (Newest First)",
                "Date (Oldest First)",
                "Amount (High to Low)",
                "Amount (Low to High)",
                "Description (A-Z)",
                "Category (A-Z)"
            };

            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, _ => SelectedTransaction != null);
            DeleteCommand = new RelayCommand(ExecuteDelete, _ => SelectedTransaction != null);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
            RowDoubleClickCommand = new RelayCommand(ExecuteEdit);

            LoadCategoryFilters();
            LoadTransactions();
        }

        #region Data Loading

        private void LoadCategoryFilters()
        {
            CategoryFilters.Clear();
            CategoryFilters.Add("All");

            foreach (var name in _categoryRepository.GetAll().OrderBy(c => c.Name).Select(c => c.Name))
                CategoryFilters.Add(name);
        }

        private void LoadTransactions()
        {
            Transactions.Clear();

            foreach (var transaction in _transactionRepository.GetAll())
                Transactions.Add(transaction);

            TotalCount = Transactions.Count;
            ApplyFilters();
        }

        #endregion

        #region Filtering & Sorting

        private void ApplyFilters()
        {
            var filtered = Transactions.AsEnumerable();

            // Search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Type
            if (SelectedTypeFilter != "All")
                filtered = filtered.Where(t => t.Type == SelectedTypeFilter);

            // Category
            if (SelectedCategoryFilter != "All")
                filtered = filtered.Where(t => t.Category == SelectedCategoryFilter);

            // Date range
            if (StartDate.HasValue)
                filtered = filtered.Where(t => t.Date >= StartDate.Value);
            if (EndDate.HasValue)
                filtered = filtered.Where(t => t.Date <= EndDate.Value);

            // Sort
            filtered = SelectedSortOption switch
            {
                "Date (Newest First)" => filtered.OrderByDescending(t => t.Date),
                "Date (Oldest First)" => filtered.OrderBy(t => t.Date),
                "Amount (High to Low)" => filtered.OrderByDescending(t => t.Amount),
                "Amount (Low to High)" => filtered.OrderBy(t => t.Amount),
                "Description (A-Z)" => filtered.OrderBy(t => t.Description),
                "Category (A-Z)" => filtered.OrderBy(t => t.Category),
                _ => filtered.OrderByDescending(t => t.Date)
            };

            FilteredTransactions.Clear();
            foreach (var transaction in filtered)
                FilteredTransactions.Add(transaction);

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            DisplayedCount = FilteredTransactions.Count;

            decimal incomeUSD = 0, expensesUSD = 0;

            foreach (var t in FilteredTransactions)
            {
                var usd = _currencyService.ConvertToUSD(t.Amount, t.Currency);

                if (t.Type == "Income")
                    incomeUSD += usd;
                else
                    expensesUSD += usd;
            }

            DisplayedIncome = incomeUSD;
            DisplayedExpenses = expensesUSD;
            DisplayedBalance = incomeUSD - expensesUSD;

            OnPropertyChanged(nameof(DisplayedIncomeFormatted));
            OnPropertyChanged(nameof(DisplayedExpensesFormatted));
            OnPropertyChanged(nameof(DisplayedBalanceFormatted));
        }

        #endregion

        #region Commands

        private void ExecuteAdd(object? parameter)
        {
            var dialog = new Views.AddEditTransactionWindow();
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadTransactions();
                MessageBox.Show("Transaction added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteEdit(object? parameter)
        {
            if (SelectedTransaction == null) return;

            var dialog = new Views.AddEditTransactionWindow(SelectedTransaction);
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadTransactions();
                MessageBox.Show("Transaction updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteDelete(object? parameter)
        {
            if (SelectedTransaction == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedTransaction.Description}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _transactionRepository.Delete(SelectedTransaction.Id);
                LoadTransactions();
                MessageBox.Show("Transaction deleted successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteClearFilters(object? parameter)
        {
            SearchText = string.Empty;
            SelectedTypeFilter = "All";
            SelectedCategoryFilter = "All";
            StartDate = null;
            EndDate = null;
            SelectedSortOption = "Date (Newest First)";
        }

        #endregion

        public void RefreshTransactions() => LoadTransactions();
    }
}