using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.Helpers;

namespace PersonalFinanceTracker.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly CurrencyService _currencyService; // FIX: Shared instance instead of creating per-method

        // Chart Series
        private ISeries[] _expensePieSeries;
        private ISeries[] _incomePieSeries;
        private ISeries[] _incomeExpenseBarSeries;
        private ISeries[] _monthlyTrendSeries;
        private ISeries[] _categoryTrendSeries;
        private ISeries[] _dayOfWeekSeries;

        // Axes
        private Axis[] _incomeExpenseXAxes;
        private Axis[] _monthlyTrendXAxes;
        private Axis[] _categoryTrendXAxes;
        private Axis[] _dayOfWeekXAxes;

        // Statistics
        private ObservableCollection<CategoryStatItem> _categoryStats;
        private ObservableCollection<IncomeSourceItem> _incomeSources;

        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _netSavings;
        private decimal _savingsRate;
        private decimal _dailyAverage;
        private int _totalTransactions;
        private int _incomeTransactions;
        private int _expenseTransactions;
        private decimal _averageIncome;
        private decimal _averageExpense;
        private decimal _largestIncome;
        private decimal _largestExpense;
        private string _mostUsedCategory;
        private int _daysAnalyzed;

        // Time Period Filter
        private string _selectedPeriod;
        private ObservableCollection<string> _timePeriods;

        // Formatted properties
        public string TotalIncomeFormatted => CurrencyFormatter.Format(TotalIncome);
        public string TotalExpensesFormatted => CurrencyFormatter.Format(TotalExpenses);
        public string NetSavingsFormatted => CurrencyFormatter.Format(NetSavings);
        public string DailyAverageFormatted => CurrencyFormatter.Format(DailyAverage);
        public string AverageIncomeFormatted => CurrencyFormatter.Format(AverageIncome);
        public string AverageExpenseFormatted => CurrencyFormatter.Format(AverageExpense);
        public string LargestIncomeFormatted => CurrencyFormatter.Format(LargestIncome);
        public string LargestExpenseFormatted => CurrencyFormatter.Format(LargestExpense);

        // Properties
        public ISeries[] ExpensePieSeries { get => _expensePieSeries; set => SetProperty(ref _expensePieSeries, value); }
        public ISeries[] IncomePieSeries { get => _incomePieSeries; set => SetProperty(ref _incomePieSeries, value); }
        public ISeries[] IncomeExpenseBarSeries { get => _incomeExpenseBarSeries; set => SetProperty(ref _incomeExpenseBarSeries, value); }
        public ISeries[] MonthlyTrendSeries { get => _monthlyTrendSeries; set => SetProperty(ref _monthlyTrendSeries, value); }
        public ISeries[] CategoryTrendSeries { get => _categoryTrendSeries; set => SetProperty(ref _categoryTrendSeries, value); }
        public ISeries[] DayOfWeekSeries { get => _dayOfWeekSeries; set => SetProperty(ref _dayOfWeekSeries, value); }

        public Axis[] IncomeExpenseXAxes { get => _incomeExpenseXAxes; set => SetProperty(ref _incomeExpenseXAxes, value); }
        public Axis[] MonthlyTrendXAxes { get => _monthlyTrendXAxes; set => SetProperty(ref _monthlyTrendXAxes, value); }
        public Axis[] CategoryTrendXAxes { get => _categoryTrendXAxes; set => SetProperty(ref _categoryTrendXAxes, value); }
        public Axis[] DayOfWeekXAxes { get => _dayOfWeekXAxes; set => SetProperty(ref _dayOfWeekXAxes, value); }

        public ObservableCollection<CategoryStatItem> CategoryStats { get => _categoryStats; set => SetProperty(ref _categoryStats, value); }
        public ObservableCollection<IncomeSourceItem> IncomeSources { get => _incomeSources; set => SetProperty(ref _incomeSources, value); }

        public decimal TotalIncome { get => _totalIncome; set => SetProperty(ref _totalIncome, value); }
        public decimal TotalExpenses { get => _totalExpenses; set => SetProperty(ref _totalExpenses, value); }
        public decimal NetSavings { get => _netSavings; set => SetProperty(ref _netSavings, value); }
        public decimal SavingsRate { get => _savingsRate; set => SetProperty(ref _savingsRate, value); }
        public decimal DailyAverage { get => _dailyAverage; set => SetProperty(ref _dailyAverage, value); }
        public int TotalTransactions { get => _totalTransactions; set => SetProperty(ref _totalTransactions, value); }
        public int IncomeTransactions { get => _incomeTransactions; set => SetProperty(ref _incomeTransactions, value); }
        public int ExpenseTransactions { get => _expenseTransactions; set => SetProperty(ref _expenseTransactions, value); }
        public decimal AverageIncome { get => _averageIncome; set => SetProperty(ref _averageIncome, value); }
        public decimal AverageExpense { get => _averageExpense; set => SetProperty(ref _averageExpense, value); }
        public decimal LargestIncome { get => _largestIncome; set => SetProperty(ref _largestIncome, value); }
        public decimal LargestExpense { get => _largestExpense; set => SetProperty(ref _largestExpense, value); }
        public string MostUsedCategory { get => _mostUsedCategory; set => SetProperty(ref _mostUsedCategory, value); }
        public int DaysAnalyzed { get => _daysAnalyzed; set => SetProperty(ref _daysAnalyzed, value); }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                    LoadChartData();
            }
        }

        public ObservableCollection<string> TimePeriods
        {
            get => _timePeriods;
            set => SetProperty(ref _timePeriods, value);
        }

        /// <summary>
        /// White paint for chart legend text on dark backgrounds.
        /// </summary>
        public SolidColorPaint LegendPaint { get; }

        public ReportsViewModel()
        {
            _transactionRepository = new TransactionRepository();
            _categoryRepository = new CategoryRepository();
            _currencyService = new CurrencyService();

            // White legend text for dark theme
            LegendPaint = new SolidColorPaint(SKColors.White);

            _expensePieSeries = Array.Empty<ISeries>();
            _incomePieSeries = Array.Empty<ISeries>();
            _incomeExpenseBarSeries = Array.Empty<ISeries>();
            _monthlyTrendSeries = Array.Empty<ISeries>();
            _categoryTrendSeries = Array.Empty<ISeries>();
            _dayOfWeekSeries = Array.Empty<ISeries>();

            _incomeExpenseXAxes = Array.Empty<Axis>();
            _monthlyTrendXAxes = Array.Empty<Axis>();
            _categoryTrendXAxes = Array.Empty<Axis>();
            _dayOfWeekXAxes = Array.Empty<Axis>();

            _categoryStats = new ObservableCollection<CategoryStatItem>();
            _incomeSources = new ObservableCollection<IncomeSourceItem>();

            _mostUsedCategory = "N/A";

            _timePeriods = new ObservableCollection<string>
            {
                "This Month",
                "Last 3 Months",
                "Last 6 Months",
                "This Year",
                "All Time"
            };
            _selectedPeriod = "Last 6 Months";

            LoadChartData();
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            var now = DateTime.Now;
            DateTime startDate;
            DateTime endDate = now;

            switch (SelectedPeriod)
            {
                case "This Month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;
                case "Last 3 Months":
                    startDate = now.AddMonths(-3);
                    break;
                case "Last 6 Months":
                    startDate = now.AddMonths(-6);
                    break;
                case "This Year":
                    startDate = new DateTime(now.Year, 1, 1);
                    break;
                case "All Time":
                default:
                    var allTransactions = _transactionRepository.GetAll();
                    startDate = allTransactions.Any()
                        ? allTransactions.Min(t => t.Date)
                        : now.AddYears(-1);
                    break;
            }

            return (startDate, endDate);
        }

        private void LoadChartData()
        {
            var (startDate, endDate) = GetDateRange();
            var transactions = _transactionRepository.GetAll()
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToList();

            // FIX: Load categories once and pass to methods that need them
            var allCategories = _categoryRepository.GetAll();

            LoadStatistics(transactions, startDate, endDate);
            LoadExpensePieChart(transactions, allCategories);
            LoadIncomePieChart(transactions, allCategories);
            LoadIncomeExpenseBarChart(transactions);
            LoadMonthlyTrendChart(transactions, startDate, endDate);
            LoadCategoryTrendChart(transactions, startDate, endDate, allCategories);
            LoadDayOfWeekChart(transactions);
            LoadCategoryStats(transactions, allCategories);
            LoadIncomeSources(transactions, allCategories);
        }

        private void LoadStatistics(List<Models.Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            TotalTransactions = transactions.Count;

            var incomeTransactions = transactions.Where(t => t.Type == "Income").ToList();
            var expenseTransactions = transactions.Where(t => t.Type == "Expense").ToList();

            IncomeTransactions = incomeTransactions.Count;
            ExpenseTransactions = expenseTransactions.Count;

            TotalIncome = incomeTransactions
                .Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency));

            TotalExpenses = expenseTransactions
                .Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency));

            NetSavings = TotalIncome - TotalExpenses;
            SavingsRate = TotalIncome > 0 ? (NetSavings / TotalIncome) * 100 : 0;

            DaysAnalyzed = (endDate - startDate).Days + 1;
            DailyAverage = DaysAnalyzed > 0 ? TotalExpenses / DaysAnalyzed : 0;

            AverageIncome = incomeTransactions.Any()
                ? incomeTransactions.Average(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))
                : 0;

            AverageExpense = expenseTransactions.Any()
                ? expenseTransactions.Average(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))
                : 0;

            LargestIncome = incomeTransactions.Any()
                ? incomeTransactions.Max(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))
                : 0;

            LargestExpense = expenseTransactions.Any()
                ? expenseTransactions.Max(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))
                : 0;

            if (transactions.Any())
            {
                MostUsedCategory = transactions
                    .GroupBy(t => t.Category)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A";
            }
            else
            {
                MostUsedCategory = "N/A";
            }

            // Notify formatted properties
            OnPropertyChanged(nameof(TotalIncomeFormatted));
            OnPropertyChanged(nameof(TotalExpensesFormatted));
            OnPropertyChanged(nameof(NetSavingsFormatted));
            OnPropertyChanged(nameof(DailyAverageFormatted));
            OnPropertyChanged(nameof(AverageIncomeFormatted));
            OnPropertyChanged(nameof(AverageExpenseFormatted));
            OnPropertyChanged(nameof(LargestIncomeFormatted));
            OnPropertyChanged(nameof(LargestExpenseFormatted));
        }

        private void LoadExpensePieChart(List<Models.Transaction> transactions, List<Models.Category> allCategories)
        {
            var expenseTransactions = transactions.Where(t => t.Type == "Expense").ToList();
            if (!expenseTransactions.Any()) { ExpensePieSeries = Array.Empty<ISeries>(); return; }

            var grouped = expenseTransactions
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency)) })
                .OrderByDescending(x => x.Total)
                .ToList();

            ExpensePieSeries = grouped.Select(item =>
            {
                var cat = allCategories.FirstOrDefault(c => c.Name == item.Category);
                return (ISeries)new PieSeries<decimal>
                {
                    Values = new[] { item.Total },
                    Name = item.Category,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => CurrencyFormatter.Format((decimal)point.PrimaryValue),
                    Fill = new SolidColorPaint(SKColor.Parse(cat?.Color ?? "#3498DB"))
                };
            }).ToArray();
        }

        private void LoadIncomePieChart(List<Models.Transaction> transactions, List<Models.Category> allCategories)
        {
            var incomeTransactions = transactions.Where(t => t.Type == "Income").ToList();
            if (!incomeTransactions.Any()) { IncomePieSeries = Array.Empty<ISeries>(); return; }

            var grouped = incomeTransactions
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency)) })
                .OrderByDescending(x => x.Total)
                .ToList();

            IncomePieSeries = grouped.Select(item =>
            {
                var cat = allCategories.FirstOrDefault(c => c.Name == item.Category);
                return (ISeries)new PieSeries<decimal>
                {
                    Values = new[] { item.Total },
                    Name = item.Category,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => CurrencyFormatter.Format((decimal)point.PrimaryValue),
                    Fill = new SolidColorPaint(SKColor.Parse(cat?.Color ?? "#27AE60"))
                };
            }).ToArray();
        }

        private void LoadIncomeExpenseBarChart(List<Models.Transaction> transactions)
        {
            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency));
            var totalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency));

            IncomeExpenseBarSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Income", Values = new[] { totalIncome },
                    Fill = new SolidColorPaint(SKColor.Parse("#27AE60")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White), DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    DataLabelsFormatter = point => CurrencyFormatter.Format((decimal)point.PrimaryValue)
                },
                new ColumnSeries<decimal>
                {
                    Name = "Expenses", Values = new[] { totalExpenses },
                    Fill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White), DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    DataLabelsFormatter = point => CurrencyFormatter.Format((decimal)point.PrimaryValue)
                }
            };

            IncomeExpenseXAxes = new Axis[]
            {
                new Axis { Labels = new[] { "Total" }, LabelsRotation = 0, TextSize = 12,
                    LabelsPaint = new SolidColorPaint(SKColors.White), SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(50)) }
            };
        }

        private void LoadMonthlyTrendChart(List<Models.Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            var months = new List<string>();
            var incomeData = new List<decimal>();
            var expenseData = new List<decimal>();

            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

            while (currentDate <= endMonth)
            {
                var monthEnd = currentDate.AddMonths(1).AddDays(-1);
                months.Add(currentDate.ToString("MMM yy"));

                incomeData.Add(transactions.Where(t => t.Type == "Income" && t.Date >= currentDate && t.Date <= monthEnd)
                    .Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency)));
                expenseData.Add(transactions.Where(t => t.Type == "Expense" && t.Date >= currentDate && t.Date <= monthEnd)
                    .Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency)));

                currentDate = currentDate.AddMonths(1);
            }

            MonthlyTrendSeries = new ISeries[]
            {
                new LineSeries<decimal> { Name = "Income", Values = incomeData, Fill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse("#27AE60")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#27AE60")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#27AE60")) { StrokeThickness = 3 },
                    GeometrySize = 10, LineSmoothness = 0.5 },
                new LineSeries<decimal> { Name = "Expenses", Values = expenseData, Fill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse("#E74C3C")) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse("#E74C3C")) { StrokeThickness = 3 },
                    GeometrySize = 10, LineSmoothness = 0.5 }
            };

            MonthlyTrendXAxes = new Axis[]
            {
                new Axis { Labels = months, LabelsRotation = -45, TextSize = 10,
                    LabelsPaint = new SolidColorPaint(SKColors.White), SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(50)) }
            };
        }

        private void LoadCategoryTrendChart(List<Models.Transaction> transactions, DateTime startDate, DateTime endDate, List<Models.Category> allCategories)
        {
            // FIX: Was using raw t.Amount without currency conversion
            var topCategories = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .OrderByDescending(g => g.Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))) // FIX: convert before sorting
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            if (!topCategories.Any())
            {
                CategoryTrendSeries = Array.Empty<ISeries>();
                CategoryTrendXAxes = Array.Empty<Axis>();
                return;
            }

            var months = new List<string>();
            var categoryDataDict = topCategories.ToDictionary(c => c, c => new List<decimal>());

            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

            while (currentDate <= endMonth)
            {
                var monthEnd = currentDate.AddMonths(1).AddDays(-1);
                months.Add(currentDate.ToString("MMM yy"));

                foreach (var category in topCategories)
                {
                    // FIX: Convert to USD for consistent comparison across currencies
                    var amount = transactions
                        .Where(t => t.Category == category && t.Date >= currentDate && t.Date <= monthEnd)
                        .Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency));

                    categoryDataDict[category].Add(amount);
                }

                currentDate = currentDate.AddMonths(1);
            }

            var seriesList = new List<ISeries>();
            var colorIndex = 0;
            var defaultColors = new[] { "#E74C3C", "#3498DB", "#F39C12", "#9B59B6", "#1ABC9C" };

            foreach (var category in topCategories)
            {
                var categoryInfo = allCategories.FirstOrDefault(c => c.Name == category);
                var color = categoryInfo?.Color ?? defaultColors[colorIndex % defaultColors.Length];

                seriesList.Add(new LineSeries<decimal>
                {
                    Name = category,
                    Values = categoryDataDict[category],
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColor.Parse(color)) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(SKColor.Parse(color)),
                    GeometryStroke = new SolidColorPaint(SKColor.Parse(color)) { StrokeThickness = 2 },
                    GeometrySize = 6,
                    LineSmoothness = 0.3
                });
                colorIndex++;
            }

            CategoryTrendSeries = seriesList.ToArray();
            CategoryTrendXAxes = new Axis[]
            {
                new Axis { Labels = months, LabelsRotation = -45, TextSize = 10,
                    LabelsPaint = new SolidColorPaint(SKColors.White), SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(50)) }
            };
        }

        private void LoadDayOfWeekChart(List<Models.Transaction> transactions)
        {
            // FIX: Convert to USD for proper cross-currency totals
            // FIX: Use CurrencyFormatter instead of hardcoded "$"
            var expensesByDay = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Date.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Day = g.Key,
                    Total = g.Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))
                })
                .ToList();

            var days = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            var amounts = new decimal[7];

            foreach (var item in expensesByDay)
                amounts[(int)item.Day] = item.Total;

            DayOfWeekSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Expenses", Values = amounts,
                    Fill = new SolidColorPaint(SKColor.Parse("#3498DB")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    // FIX: Use CurrencyFormatter instead of hardcoded "$"
                    DataLabelsFormatter = point => point.PrimaryValue > 0
                        ? CurrencyFormatter.Format((decimal)point.PrimaryValue)
                        : ""
                }
            };

            DayOfWeekXAxes = new Axis[]
            {
                new Axis { Labels = days, LabelsRotation = 0, TextSize = 11,
                    LabelsPaint = new SolidColorPaint(SKColors.White), SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(50)) }
            };
        }

        private void LoadCategoryStats(List<Models.Transaction> transactions, List<Models.Category> allCategories)
        {
            CategoryStats.Clear();

            var categoryGroups = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency)),
                    Count = g.Count(),
                    Average = g.Average(t => _currencyService.ConvertToUSD(t.Amount, t.Currency))
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var totalExpenses = categoryGroups.Sum(x => x.Total);

            foreach (var item in categoryGroups)
            {
                var category = allCategories.FirstOrDefault(c => c.Name == item.Category);
                var percentage = totalExpenses > 0 ? (item.Total / totalExpenses) * 100 : 0;

                CategoryStats.Add(new CategoryStatItem
                {
                    Category = item.Category,
                    Icon = category?.Icon ?? "📦",
                    Color = category?.Color ?? "#95A5A6",
                    Total = item.Total,
                    Count = item.Count,
                    Average = item.Average,
                    Percentage = percentage
                });
            }
        }

        private void LoadIncomeSources(List<Models.Transaction> transactions, List<Models.Category> allCategories)
        {
            IncomeSources.Clear();

            // FIX: Convert to USD for proper cross-currency totals
            // Old code used raw t.Amount without conversion
            var incomeGroups = transactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => _currencyService.ConvertToUSD(t.Amount, t.Currency)),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var totalIncome = incomeGroups.Sum(x => x.Total);

            foreach (var item in incomeGroups)
            {
                var category = allCategories.FirstOrDefault(c => c.Name == item.Category);
                var percentage = totalIncome > 0 ? (item.Total / totalIncome) * 100 : 0;

                IncomeSources.Add(new IncomeSourceItem
                {
                    Source = item.Category,
                    Icon = category?.Icon ?? "💰",
                    Color = category?.Color ?? "#27AE60",
                    Total = item.Total,
                    Count = item.Count,
                    Percentage = percentage
                });
            }
        }

        public void RefreshCharts()
        {
            LoadChartData();
        }
    }

    // Helper classes
    public class CategoryStatItem : BaseViewModel
    {
        private string _category = string.Empty;
        private string _icon = string.Empty;
        private string _color = string.Empty;
        private decimal _total;
        private int _count;
        private decimal _average;
        private decimal _percentage;

        public string Category { get => _category; set => SetProperty(ref _category, value); }
        public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
        public string Color { get => _color; set => SetProperty(ref _color, value); }

        public decimal Total
        {
            get => _total;
            set { if (SetProperty(ref _total, value)) OnPropertyChanged(nameof(TotalFormatted)); }
        }

        public int Count { get => _count; set => SetProperty(ref _count, value); }

        public decimal Average
        {
            get => _average;
            set { if (SetProperty(ref _average, value)) OnPropertyChanged(nameof(AverageFormatted)); }
        }

        public decimal Percentage { get => _percentage; set => SetProperty(ref _percentage, value); }

        public string TotalFormatted => CurrencyFormatter.Format(Total);
        public string AverageFormatted => CurrencyFormatter.Format(Average);
    }

    public class IncomeSourceItem : BaseViewModel
    {
        private string _source = string.Empty;
        private string _icon = string.Empty;
        private string _color = string.Empty;
        private decimal _total;
        private int _count;
        private decimal _percentage;

        public string Source { get => _source; set => SetProperty(ref _source, value); }
        public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
        public string Color { get => _color; set => SetProperty(ref _color, value); }

        public decimal Total
        {
            get => _total;
            set { if (SetProperty(ref _total, value)) OnPropertyChanged(nameof(TotalFormatted)); }
        }

        // FIX: Added TotalFormatted property — XAML was falling back to StringFormat=C
        // which uses system locale instead of user's selected currency
        public string TotalFormatted => CurrencyFormatter.Format(Total);

        public int Count { get => _count; set => SetProperty(ref _count, value); }
        public decimal Percentage { get => _percentage; set => SetProperty(ref _percentage, value); }
    }
}