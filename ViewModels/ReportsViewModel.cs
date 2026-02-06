using System.Collections.Generic;
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

        private ISeries[] _expensePieSeries;
        private ISeries[] _incomeExpenseBarSeries;
        private Axis[] _incomeExpenseXAxes;
        private bool _hasData;

        public ISeries[] ExpensePieSeries
        {
            get => _expensePieSeries;
            set => SetProperty(ref _expensePieSeries, value);
        }

        public ISeries[] IncomeExpenseBarSeries
        {
            get => _incomeExpenseBarSeries;
            set => SetProperty(ref _incomeExpenseBarSeries, value);
        }

        public Axis[] IncomeExpenseXAxes
        {
            get => _incomeExpenseXAxes;
            set => SetProperty(ref _incomeExpenseXAxes, value);
        }

        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        public ReportsViewModel()
        {
            _transactionRepository = new TransactionRepository();
            _categoryRepository = new CategoryRepository();

            _expensePieSeries = System.Array.Empty<ISeries>();
            _incomeExpenseBarSeries = System.Array.Empty<ISeries>();
            _incomeExpenseXAxes = System.Array.Empty<Axis>();

            LoadChartData();
        }

        private void LoadChartData()
        {
            var transactions = _transactionRepository.GetAll();
            HasData = transactions.Any();

            if (HasData)
            {
                LoadExpensePieChart();
                LoadIncomeExpenseBarChart();
            }
        }

        private void LoadExpensePieChart()
        {
            var transactions = _transactionRepository.GetAll();
            var expenseTransactions = transactions.Where(t => t.Type == "Expense").ToList();

            if (!expenseTransactions.Any())
            {
                ExpensePieSeries = System.Array.Empty<ISeries>();
                return;
            }

            // Group by category and sum amounts
            var groupedExpenses = expenseTransactions
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            var pieSeriesList = new List<ISeries>();

            foreach (var item in groupedExpenses)
            {
                // Get category color
                var category = _categoryRepository.GetAll()
                    .FirstOrDefault(c => c.Name == item.Category);

                var colorHex = category?.Color ?? "#3498DB";
                var color = SKColor.Parse(colorHex);

                pieSeriesList.Add(new PieSeries<decimal>
                {
                    Values = new[] { item.Total },
                    Name = item.Category,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.PrimaryValue:N0}",
                    Fill = new SolidColorPaint(color)
                });
            }

            ExpensePieSeries = pieSeriesList.ToArray();
        }

        private void LoadIncomeExpenseBarChart()
        {
            var totalIncome = _transactionRepository.GetTotalIncome();
            var totalExpenses = _transactionRepository.GetTotalExpenses();

            IncomeExpenseBarSeries = new ISeries[]
            {
                new ColumnSeries<decimal>
                {
                    Name = "Income",
                    Values = new[] { totalIncome },
                    Fill = new SolidColorPaint(SKColor.Parse("#27AE60")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"${point.PrimaryValue:N0}"
                },
                new ColumnSeries<decimal>
                {
                    Name = "Expenses",
                    Values = new[] { totalExpenses },
                    Fill = new SolidColorPaint(SKColor.Parse("#E74C3C")),
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 16,
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
                    TextSize = 14,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray.WithAlpha(100))
                }
            };
        }

        public void RefreshCharts()
        {
            LoadChartData();
        }
    }
}