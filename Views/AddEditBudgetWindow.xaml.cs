using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Views
{
    public partial class AddEditBudgetWindow : Window
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly BudgetRepository _budgetRepository;
        private Budget? _budget;
        private bool _isEditMode;

        public bool IsSaved { get; private set; }

        // Constructor for ADD mode
        public AddEditBudgetWindow()
        {
            InitializeComponent();
            _categoryRepository = new CategoryRepository();
            _budgetRepository = new BudgetRepository();
            _isEditMode = false;

            LoadExpenseCategories();
        }

        // Constructor for EDIT mode
        public AddEditBudgetWindow(Budget budget) : this()
        {
            _budget = budget;
            _isEditMode = true;
            Title = "Edit Budget";

            // Populate fields
            BudgetAmountTextBox.Text = budget.BudgetAmount.ToString();

            // Set Period
            PeriodComboBox.SelectedIndex = budget.Period switch
            {
                "Monthly" => 0,
                "Weekly" => 1,
                "Yearly" => 2,
                _ => 0
            };

            // Select category
            var categoryToSelect = CategoryComboBox.Items.Cast<Category>()
                .FirstOrDefault(c => c.Name == budget.Category);
            if (categoryToSelect != null)
            {
                CategoryComboBox.SelectedItem = categoryToSelect;
            }
        }
        private void BudgetAmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers and decimal point
            Regex regex = new Regex(@"^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void LoadExpenseCategories()
        {
            CategoryComboBox.Items.Clear();
            var categories = _categoryRepository.GetByType("Expense");
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }
            if (CategoryComboBox.Items.Count > 0)
            {
                CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sanitize input
            BudgetAmountTextBox.Text = ValidationHelper.SanitizeInput(BudgetAmountTextBox.Text);

            // Validation
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidDecimal(BudgetAmountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid budget amount.\nExample: 500.00",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                BudgetAmountTextBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidAmount(amount))
            {
                MessageBox.Show("Budget amount must be between $0.01 and $999,999,999.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                BudgetAmountTextBox.Focus();
                return;
            }

            try
            {
                var selectedCategory = (Category)CategoryComboBox.SelectedItem;
                var selectedPeriod = ((ComboBoxItem)PeriodComboBox.SelectedItem).Content.ToString() ?? "Monthly";

                // Check if budget already exists for this category
                if (!_isEditMode)
                {
                    var existingBudget = _budgetRepository.GetByCategory(selectedCategory.Name);
                    if (existingBudget != null)
                    {
                        MessageBox.Show(
                            $"A budget for '{selectedCategory.Name}' already exists.\nPlease edit the existing budget instead.",
                            "Duplicate Budget",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                }

                if (_isEditMode && _budget != null)
                {
                    // UPDATE existing budget
                    _budget.Category = selectedCategory.Name;
                    _budget.BudgetAmount = amount;
                    _budget.Period = selectedPeriod;

                    _budgetRepository.Update(_budget);
                }
                else
                {
                    // ADD new budget
                    var newBudget = new Budget
                    {
                        Category = selectedCategory.Name,
                        BudgetAmount = amount,
                        Period = selectedPeriod
                    };

                    _budgetRepository.Add(newBudget);
                }

                IsSaved = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while saving the budget:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            Close();
        }
    }
}
