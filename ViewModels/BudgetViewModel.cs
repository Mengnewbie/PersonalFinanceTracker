using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly BudgetRepository _budgetRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly CurrencyService _currencyService;

        private ObservableCollection<BudgetItemViewModel> _budgetItems;
        private BudgetItemViewModel? _selectedBudgetItem;

        public ObservableCollection<BudgetItemViewModel> BudgetItems
        {
            get => _budgetItems;
            set => SetProperty(ref _budgetItems, value);
        }

        public BudgetItemViewModel? SelectedBudgetItem
        {
            get => _selectedBudgetItem;
            set => SetProperty(ref _selectedBudgetItem, value);
        }

        public ICommand AddBudgetCommand { get; }
        public ICommand EditBudgetCommand { get; }
        public ICommand DeleteBudgetCommand { get; }

        public BudgetViewModel()
        {
            _budgetRepository = new BudgetRepository();
            _transactionRepository = new TransactionRepository();
            _categoryRepository = new CategoryRepository();
            _currencyService = new CurrencyService();
            _budgetItems = new ObservableCollection<BudgetItemViewModel>();

            AddBudgetCommand = new RelayCommand(ExecuteAddBudget);
            EditBudgetCommand = new RelayCommand(ExecuteEditBudget, _ => SelectedBudgetItem != null);
            DeleteBudgetCommand = new RelayCommand(ExecuteDeleteBudget, _ => SelectedBudgetItem != null);

            LoadBudgets();
        }

        private void LoadBudgets()
        {
            BudgetItems.Clear();

            var budgets = _budgetRepository.GetAll();
            var transactions = _transactionRepository.GetAll();
            var allCategories = _categoryRepository.GetAll(); // FIX: Load once, not per-budget

            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            foreach (var budget in budgets)
            {
                // Sum expenses for this category, converting each to budget currency
                var spentInBudgetCurrency = transactions
                    .Where(t => t.Category == budget.Category
                             && t.Type == "Expense"
                             && t.Date >= monthStart
                             && t.Date <= monthEnd)
                    .Sum(t => _currencyService.Convert(t.Amount, t.Currency, budget.Currency));

                var category = allCategories.FirstOrDefault(c => c.Name == budget.Category);

                BudgetItems.Add(new BudgetItemViewModel(_currencyService)
                {
                    BudgetId = budget.Id,
                    Category = budget.Category,
                    BudgetAmount = budget.BudgetAmount,
                    BudgetCurrency = budget.Currency,
                    Spent = spentInBudgetCurrency,
                    Icon = category?.Icon ?? "📊"
                });
            }
        }

        private void ExecuteAddBudget(object? parameter)
        {
            var dialog = new Views.AddEditBudgetWindow();
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadBudgets();
                MessageBox.Show("Budget added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteEditBudget(object? parameter)
        {
            if (SelectedBudgetItem == null) return;

            var budget = _budgetRepository.GetAll()
                .FirstOrDefault(b => b.Id == SelectedBudgetItem.BudgetId);
            if (budget == null) return;

            var dialog = new Views.AddEditBudgetWindow(budget);
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadBudgets();
                MessageBox.Show("Budget updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteDeleteBudget(object? parameter)
        {
            if (SelectedBudgetItem == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the budget for '{SelectedBudgetItem.Category}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _budgetRepository.Delete(SelectedBudgetItem.BudgetId);
                LoadBudgets();
                MessageBox.Show("Budget deleted successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void RefreshBudgets() => LoadBudgets();
    }
}