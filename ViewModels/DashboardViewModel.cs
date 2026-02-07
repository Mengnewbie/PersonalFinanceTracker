using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly BudgetRepository _budgetRepository;
        private readonly CategoryRepository _categoryRepository;

        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _balance;
        private decimal _thisMonthIncome;
        private decimal _thisMonthExpenses;
        private decimal _lastMonthIncome;
        private decimal _lastMonthExpenses;
        private string _incomeChange;
        private string _expenseChange;
        private string _incomeChangeColor;
        private string _expenseChangeColor;

        private ObservableCollection<Transaction> _recentTransactions;
        private ObservableCollection<CategorySpendingItem> _topCategories;

        private int _budgetsOnTrack;
        private int _budgetsWarning;
        private int _budgetsOverBudget;
        private int _totalBudgets;

        private decimal _averageTransaction;
        private decimal _largestExpense;
        private int _transactionCount;

        private ISeries[] _monthlyTrendSeries;
        private Axis[] _monthlyTrendXAxes;

        // Properties
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

        public decimal ThisMonthIncome
        {
            get => _thisMonthIncome;
            set => SetProperty(ref _thisMonthIncome, value);
        }

        public decimal ThisMonthExpenses
        {
            get => _thisMonthExpenses;
            set => SetProperty(ref _thisMonthExpenses, value);
        }

        public decimal LastMonthIncome
        {
            get => _lastMonthIncome;
            set => SetProperty(ref _lastMonthIncome, value);
        }

        public decimal LastMonthExpenses
        {
            get => _lastMonthExpenses;
            set => SetProperty(ref _lastMonthExpenses, value);
        }

        public string IncomeChange
        {
            get => _incomeChange;
            set => SetProperty(ref _incomeChange, value);
        }

        public string ExpenseChange
        {
            get => _expenseChange;
            set => SetProperty(ref _expenseChange, value);
        }

        public string IncomeChangeColor
        {
            get => _incomeChangeColor;
            set => SetProperty(ref _incomeChangeColor, value);
        }

        public string ExpenseChangeColor
        {
            get => _expenseChangeColor;
            set => SetProperty(ref _expenseChangeColor, value);
        }

        public ObservableCollection<Transaction> RecentTransactions
        {
            get => _recentTransactions;
            set => SetProperty(ref _recentTransactions, value);
        }

        public ObservableCollection<CategorySpendingItem> TopCategories
        {
            get => _topCategories;
            set => SetProperty(ref _topCategories, value);
        }

        public int BudgetsOnTrack
        {
            get => _budgetsOnTrack;
            set => SetProperty(ref _budgetsOnTrack, value);
        }

        public int BudgetsWarning
        {
            get => _budgetsWarning;
            set => SetProperty(ref _budgetsWarning, value);
        }

        public int BudgetsOverBudget
        {
            get => _budgetsOverBudget;
            set => SetProperty(ref _budgetsOverBudget, value);
        }

        public int TotalBudgets
        {
            get => _totalBudgets;
            set => SetProperty(ref _totalBudgets, value);
        }

        public decimal AverageTransaction
        {
            get => _averageTransaction;
            set => SetProperty(ref _averageTransaction, value);
        }

        public decimal LargestExpense
        {
            get => _largestExpense;
            set => SetProperty(ref _largestExpense, value);
        }

        public int TransactionCount
        {
            get => _transactionCount;
            set => SetProperty(ref _transactionCount, value);
        }

        public ISeries[] MonthlyTrendSeries
        {
            get => _monthlyTrendSeries;
            set => SetProperty(ref _monthlyTrendSeries, value);
        }

        public Axis[] MonthlyTrendXAxes
        {
            get => _monthlyTrendXAxes;
            set => SetProperty(ref _monthlyTrendXAxes, value);
        }

        public DashboardViewModel()
        {
            _transactionRepository = new TransactionRepository();
            _budgetRepository = new BudgetRepository();
            _categoryRepository = new CategoryRepository();

            _recentTransactions = new ObservableCollection<Transaction>();
            _topCategories = new ObservableCollection<CategorySpendingItem>();
            _monthlyTrendSeries = Array.Empty<ISeries>();
            _monthlyTrendXAxes = Array.Empty<Axis>();
            _incomeChange = "";
            _expenseChange = "";
            _incomeChangeColor = "#27AE60";
            _expenseChangeColor = "#E74C3C";

            LoadData();
        }

        private void LoadData()
        {
            LoadTotals();
            LoadMonthlyComparison();
            LoadRecentTransactions();
            LoadTopCategories();
            LoadBudgetSummary();
            LoadQuickStats();
            LoadMonthlyTrend();
        }

        private void LoadTotals()
        {
            TotalIncome = _transactionRepository.GetTotalIncome();
            TotalExpenses = _transactionRepository.GetTotalExpenses();
            Balance = TotalIncome - TotalExpenses;
        }

        private void LoadMonthlyComparison()
        {
            var transactions = _transactionRepository.GetAll();
            var now = DateTime.Now;

            // This month
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

            ThisMonthIncome = transactions
                .Where(t => t.Type == "Income" && t.Date >= thisMonthStart && t.Date <= thisMonthEnd)
                .Sum(t => t.Amount);

            ThisMonthExpenses = transactions
                .Where(t => t.Type == "Expense" && t.Date >= thisMonthStart && t.Date <= thisMonthEnd)
                .Sum(t => t.Amount);

            // Last month
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            LastMonthIncome = transactions
                .Where(t => t.Type == "Income" && t.Date >= lastMonthStart && t.Date <= lastMonthEnd)
                .Sum(t => t.Amount);

            LastMonthExpenses = transactions
                .Where(t => t.Type == "Expense" && t.Date >= lastMonthStart && t.Date <= lastMonthEnd)
                .Sum(t => t.Amount);

            // Calculate changes
            if (LastMonthIncome > 0)
            {
                var incomeChangePct = ((ThisMonthIncome - LastMonthIncome) / LastMonthIncome) * 100;
                IncomeChange = incomeChangePct >= 0
                    ? $"↑ {incomeChangePct:F1}% vs last month"
                    : $"↓ {Math.Abs(incomeChangePct):F1}% vs last month";
                IncomeChangeColor = incomeChangePct >= 0 ? "#27AE60" : "#E74C3C";
            }
            else
            {
                IncomeChange = ThisMonthIncome > 0 ? "New income this month!" : "No income yet";
                IncomeChangeColor = "#95A5A6";
            }

            if (LastMonthExpenses > 0)
            {
                var expenseChangePct = ((ThisMonthExpenses - LastMonthExpenses) / LastMonthExpenses) * 100;
                ExpenseChange = expenseChangePct >= 0
                    ? $"↑ {expenseChangePct:F1}% vs last month"
                    : $"↓ {Math.Abs(expenseChangePct):F1}% vs last month";
                // Note: For expenses, increase is bad (red), decrease is good (green)
                ExpenseChangeColor = expenseChangePct >= 0 ? "#E74C3C" : "#27AE60";
            }
            else
            {
                ExpenseChange = ThisMonthExpenses > 0 ? "New expenses this month!" : "No expenses yet";
                ExpenseChangeColor = "#95A5A6";
            }
        }

        private void LoadRecentTransactions()
        {
            RecentTransactions.Clear();
            var transactions = _transactionRepository.GetAll()
                .OrderByDescending(t => t.Date)
                .Take(5);

            foreach (var transaction in transactions)
            {
                RecentTransactions.Add(transaction);
            }
        }

        private void LoadTopCategories()
        {
            TopCategories.Clear();
            var transactions = _transactionRepository.GetAll();
            var now = DateTime.Now;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);

            var topSpending = transactions
                .Where(t => t.Type == "Expense" && t.Date >= thisMonthStart)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Amount = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .Take(3);

            foreach (var item in topSpending)
            {
                var category = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == item.Category);

                TopCategories.Add(new CategorySpendingItem
                {
                    Category = item.Category,
                    Amount = item.Amount,
                    TransactionCount = item.Count,
                    Icon = category?.Icon ?? "📦",
                    Color = category?.Color ?? "#95A5A6"
                });
            }
        }

        private void LoadBudgetSummary()
        {
            var budgets = _budgetRepository.GetAll();
            var transactions = _transactionRepository.GetAll();
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            TotalBudgets = budgets.Count;
            BudgetsOnTrack = 0;
            BudgetsWarning = 0;
            BudgetsOverBudget = 0;

            foreach (var budget in budgets)
            {
                var spent = transactions
                    .Where(t => t.Category == budget.Category
                             && t.Type == "Expense"
                             && t.Date >= monthStart
                             && t.Date <= monthEnd)
                    .Sum(t => t.Amount);

                var percentage = budget.BudgetAmount > 0
                    ? (spent / budget.BudgetAmount) * 100
                    : 0;

                if (percentage >= 100)
                    BudgetsOverBudget++;
                else if (percentage >= 80)
                    BudgetsWarning++;
                else
                    BudgetsOnTrack++;
            }
        }

        private void LoadQuickStats()
        {
            var transactions = _transactionRepository.GetAll();
            TransactionCount = transactions.Count;

            if (transactions.Any())
            {
                AverageTransaction = transactions.Average(t => t.Amount);

                var expenses = transactions.Where(t => t.Type == "Expense").ToList();
                LargestExpense = expenses.Any() ? expenses.Max(t => t.Amount) : 0;
            }
            else
            {
                AverageTransaction = 0;
                LargestExpense = 0;
            }
        }

        private void LoadMonthlyTrend()
        {
            var transactions = _transactionRepository.GetAll();
            var now = DateTime.Now;
            var months = new List<string>();
            var incomeData = new List<decimal>();
            var expenseData = new List<decimal>();

            // Get last 6 months
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                months.Add(month.ToString("MMM"));

                var monthIncome = transactions
                    .Where(t => t.Type == "Income" && t.Date >= monthStart && t.Date <= monthEnd)
                    .Sum(t => t.Amount);

                var monthExpense = transactions
                    .Where(t => t.Type == "Expense" && t.Date >= monthStart && t.Date <= monthEnd)
                    .Sum(t => t.Amount);

                incomeData.Add(monthIncome);
                expenseData.Add(monthExpense);
            }

            MonthlyTrendSeries = new ISeries[]
            {
                new LineSeries<decimal>
                {
                    Name = "Income",
                    Values = incomeData,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse("#27AE60")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#27AE60")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#27AE60")) { StrokeThickness = 3 },
                    GeometrySize = 8,
                    LineSmoothness = 0.5
                },
                new LineSeries<decimal>
                {
                    Name = "Expenses",
                    Values = expenseData,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse("#E74C3C")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#E74C3C")) { StrokeThickness = 3 },
                    GeometrySize = 8,
                    LineSmoothness = 0.5
                }
            };

            MonthlyTrendXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = months,
                    LabelsRotation = 0,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
                }
            };
        }

        public void RefreshData()
        {
            LoadData();
        }
    }

    // Helper class for top categories
    public class CategorySpendingItem : BaseViewModel
    {
        private string _category = string.Empty;
        private decimal _amount;
        private int _transactionCount;
        private string _icon = string.Empty;
        private string _color = string.Empty;

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public int TransactionCount
        {
            get => _transactionCount;
            set => SetProperty(ref _transactionCount, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
    }
}