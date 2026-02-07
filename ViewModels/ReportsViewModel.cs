using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly CategoryRepository _categoryRepository;

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

        // Properties
        public ISeries[] ExpensePieSeries
        {
            get => _expensePieSeries;
            set => SetProperty(ref _expensePieSeries, value);
        }

        public ISeries[] IncomePieSeries
        {
            get => _incomePieSeries;
            set => SetProperty(ref _incomePieSeries, value);
        }

        public ISeries[] IncomeExpenseBarSeries
        {
            get => _incomeExpenseBarSeries;
            set => SetProperty(ref _incomeExpenseBarSeries, value);
        }

        public ISeries[] MonthlyTrendSeries
        {
            get => _monthlyTrendSeries;
            set => SetProperty(ref _monthlyTrendSeries, value);
        }

        public ISeries[] CategoryTrendSeries
        {
            get => _categoryTrendSeries;
            set => SetProperty(ref _categoryTrendSeries, value);
        }

        public ISeries[] DayOfWeekSeries
        {
            get => _dayOfWeekSeries;
            set => SetProperty(ref _dayOfWeekSeries, value);
        }

        public Axis[] IncomeExpenseXAxes
        {
            get => _incomeExpenseXAxes;
            set => SetProperty(ref _incomeExpenseXAxes, value);
        }

        public Axis[] MonthlyTrendXAxes
        {
            get => _monthlyTrendXAxes;
            set => SetProperty(ref _monthlyTrendXAxes, value);
        }

        public Axis[] CategoryTrendXAxes
        {
            get => _categoryTrendXAxes;
            set => SetProperty(ref _categoryTrendXAxes, value);
        }

        public Axis[] DayOfWeekXAxes
        {
            get => _dayOfWeekXAxes;
            set => SetProperty(ref _dayOfWeekXAxes, value);
        }

        public ObservableCollection<CategoryStatItem> CategoryStats
        {
            get => _categoryStats;
            set => SetProperty(ref _categoryStats, value);
        }

        public ObservableCollection<IncomeSourceItem> IncomeSources
        {
            get => _incomeSources;
            set => SetProperty(ref _incomeSources, value);
        }

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

        public decimal NetSavings
        {
            get => _netSavings;
            set => SetProperty(ref _netSavings, value);
        }

        public decimal SavingsRate
        {
            get => _savingsRate;
            set => SetProperty(ref _savingsRate, value);
        }

        public decimal DailyAverage
        {
            get => _dailyAverage;
            set => SetProperty(ref _dailyAverage, value);
        }

        public int TotalTransactions
        {
            get => _totalTransactions;
            set => SetProperty(ref _totalTransactions, value);
        }

        public int IncomeTransactions
        {
            get => _incomeTransactions;
            set => SetProperty(ref _incomeTransactions, value);
        }

        public int ExpenseTransactions
        {
            get => _expenseTransactions;
            set => SetProperty(ref _expenseTransactions, value);
        }

        public decimal AverageIncome
        {
            get => _averageIncome;
            set => SetProperty(ref _averageIncome, value);
        }

        public decimal AverageExpense
        {
            get => _averageExpense;
            set => SetProperty(ref _averageExpense, value);
        }

        public decimal LargestIncome
        {
            get => _largestIncome;
            set => SetProperty(ref _largestIncome, value);
        }

        public decimal LargestExpense
        {
            get => _largestExpense;
            set => SetProperty(ref _largestExpense, value);
        }

        public string MostUsedCategory
        {
            get => _mostUsedCategory;
            set => SetProperty(ref _mostUsedCategory, value);
        }

        public int DaysAnalyzed
        {
            get => _daysAnalyzed;
            set => SetProperty(ref _daysAnalyzed, value);
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (SetProperty(ref _selectedPeriod, value))
                {
                    LoadChartData();
                }
            }
        }

        public ObservableCollection<string> TimePeriods
        {
            get => _timePeriods;
            set => SetProperty(ref _timePeriods, value);
        }

        public ReportsViewModel()
        {
            _transactionRepository = new TransactionRepository();
            _categoryRepository = new CategoryRepository();

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

            // Initialize time periods
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

            LoadStatistics(transactions, startDate, endDate);
            LoadExpensePieChart(transactions);
            LoadIncomePieChart(transactions);
            LoadIncomeExpenseBarChart(transactions);
            LoadMonthlyTrendChart(transactions, startDate, endDate);
            LoadCategoryTrendChart(transactions, startDate, endDate);
            LoadDayOfWeekChart(transactions);
            LoadCategoryStats(transactions);
            LoadIncomeSources(transactions);
        }

        private void LoadStatistics(List<Models.Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            TotalTransactions = transactions.Count;

            var incomeTransactions = transactions.Where(t => t.Type == "Income").ToList();
            var expenseTransactions = transactions.Where(t => t.Type == "Expense").ToList();

            IncomeTransactions = incomeTransactions.Count;
            ExpenseTransactions = expenseTransactions.Count;

            TotalIncome = incomeTransactions.Sum(t => t.Amount);
            TotalExpenses = expenseTransactions.Sum(t => t.Amount);
            NetSavings = TotalIncome - TotalExpenses;
            SavingsRate = TotalIncome > 0 ? (NetSavings / TotalIncome) * 100 : 0;

            DaysAnalyzed = (endDate - startDate).Days + 1;
            DailyAverage = DaysAnalyzed > 0 ? TotalExpenses / DaysAnalyzed : 0;

            AverageIncome = incomeTransactions.Any() ? incomeTransactions.Average(t => t.Amount) : 0;
            AverageExpense = expenseTransactions.Any() ? expenseTransactions.Average(t => t.Amount) : 0;

            LargestIncome = incomeTransactions.Any() ? incomeTransactions.Max(t => t.Amount) : 0;
            LargestExpense = expenseTransactions.Any() ? expenseTransactions.Max(t => t.Amount) : 0;

            if (transactions.Any())
            {
                var categoryGroups = transactions
                    .GroupBy(t => t.Category)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                MostUsedCategory = categoryGroups?.Key ?? "N/A";
            }
            else
            {
                MostUsedCategory = "N/A";
            }
        }

        private void LoadExpensePieChart(List<Models.Transaction> transactions)
        {
            var expenseTransactions = transactions.Where(t => t.Type == "Expense").ToList();

            if (!expenseTransactions.Any())
            {
                ExpensePieSeries = Array.Empty<ISeries>();
                return;
            }

            var groupedExpenses = expenseTransactions
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var pieSeriesList = new List<ISeries>();

            foreach (var item in groupedExpenses)
            {
                var category = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == item.Category);

                var colorHex = category?.Color ?? "#3498DB";
                var color = SKColor.Parse(colorHex);

                pieSeriesList.Add(new PieSeries<decimal>
                {
                    Values = new[] { item.Total },
                    Name = item.Category,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.PrimaryValue:N0}",
                    Fill = new SolidColorPaint(color)
                });
            }

            ExpensePieSeries = pieSeriesList.ToArray();
        }

        private void LoadIncomePieChart(List<Models.Transaction> transactions)
        {
            var incomeTransactions = transactions.Where(t => t.Type == "Income").ToList();

            if (!incomeTransactions.Any())
            {
                IncomePieSeries = Array.Empty<ISeries>();
                return;
            }

            var groupedIncome = incomeTransactions
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var pieSeriesList = new List<ISeries>();

            foreach (var item in groupedIncome)
            {
                var category = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == item.Category);

                var colorHex = category?.Color ?? "#27AE60";
                var color = SKColor.Parse(colorHex);

                pieSeriesList.Add(new PieSeries<decimal>
                {
                    Values = new[] { item.Total },
                    Name = item.Category,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.PrimaryValue:N0}",
                    Fill = new SolidColorPaint(color)
                });
            }

            IncomePieSeries = pieSeriesList.ToArray();
        }

        private void LoadIncomeExpenseBarChart(List<Models.Transaction> transactions)
        {
            var totalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount);

            IncomeExpenseBarSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Income",
                    Values = new[] { totalIncome },
                    Fill = new SolidColorPaint(SKColor.Parse("#27AE60")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.PrimaryValue:N0}"
                },
                new ColumnSeries<decimal>
                {
                    Name = "Expenses",
                    Values = new[] { totalExpenses },
                    Fill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.PrimaryValue:N0}"
                }
            };

            IncomeExpenseXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new[] { "Total" },
                    LabelsRotation = 0,
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
                }
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

                var monthIncome = transactions
                    .Where(t => t.Type == "Income" && t.Date >= currentDate && t.Date <= monthEnd)
                    .Sum(t => t.Amount);

                var monthExpense = transactions
                    .Where(t => t.Type == "Expense" && t.Date >= currentDate && t.Date <= monthEnd)
                    .Sum(t => t.Amount);

                incomeData.Add(monthIncome);
                expenseData.Add(monthExpense);

                currentDate = currentDate.AddMonths(1);
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
                    GeometrySize = 10,
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
                    GeometrySize = 10,
                    LineSmoothness = 0.5
                }
            };

            MonthlyTrendXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = months,
                    LabelsRotation = -45,
                    TextSize = 10,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
                }
            };
        }

        private void LoadCategoryTrendChart(List<Models.Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            var topCategories = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .OrderByDescending(g => g.Sum(t => t.Amount))
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
                    var amount = transactions
                        .Where(t => t.Category == category && t.Date >= currentDate && t.Date <= monthEnd)
                        .Sum(t => t.Amount);

                    categoryDataDict[category].Add(amount);
                }

                currentDate = currentDate.AddMonths(1);
            }

            var seriesList = new List<ISeries>();
            var colorIndex = 0;
            var colors = new[] { "#E74C3C", "#3498DB", "#F39C12", "#9B59B6", "#1ABC9C" };

            foreach (var category in topCategories)
            {
                var categoryInfo = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == category);

                var color = categoryInfo?.Color ?? colors[colorIndex % colors.Length];

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
                new Axis
                {
                    Labels = months,
                    LabelsRotation = -45,
                    TextSize = 10,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
                }
            };
        }

        private void LoadDayOfWeekChart(List<Models.Transaction> transactions)
        {
            var expensesByDay = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Date.DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Day = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToList();

            var days = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            var amounts = new decimal[7];

            foreach (var item in expensesByDay)
            {
                amounts[(int)item.Day] = item.Total;
            }

            DayOfWeekSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Expenses",
                    Values = amounts,
                    Fill = new SolidColorPaint(SKColor.Parse("#3498DB")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 11,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                    DataLabelsFormatter = point => point.PrimaryValue > 0 ? $"${point.PrimaryValue:N0}" : ""
                }
            };

            DayOfWeekXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = days,
                    LabelsRotation = 0,
                    TextSize = 11,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
                }
            };
        }

        private void LoadCategoryStats(List<Models.Transaction> transactions)
        {
            CategoryStats.Clear();

            var categoryGroups = transactions
                .Where(t => t.Type == "Expense")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Count = g.Count(),
                    Average = g.Average(t => t.Amount),
                    Percentage = 0m
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var totalExpenses = categoryGroups.Sum(x => x.Total);

            foreach (var item in categoryGroups)
            {
                var category = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == item.Category);

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

        private void LoadIncomeSources(List<Models.Transaction> transactions)
        {
            IncomeSources.Clear();

            var incomeGroups = transactions
                .Where(t => t.Type == "Income")
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            var totalIncome = incomeGroups.Sum(x => x.Total);

            foreach (var item in incomeGroups)
            {
                var category = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == item.Category);

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

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
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

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public decimal Average
        {
            get => _average;
            set => SetProperty(ref _average, value);
        }

        public decimal Percentage
        {
            get => _percentage;
            set => SetProperty(ref _percentage, value);
        }
    }

    public class IncomeSourceItem : BaseViewModel
    {
        private string _source = string.Empty;
        private string _icon = string.Empty;
        private string _color = string.Empty;
        private decimal _total;
        private int _count;
        private decimal _percentage;

        public string Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
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

        public decimal Total
        {
            get => _total;
            set => SetProperty(ref _total, value);
        }

        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public decimal Percentage
        {
            get => _percentage;
            set => SetProperty(ref _percentage, value);
        }
    }
}