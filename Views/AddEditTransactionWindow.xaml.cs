using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.Helpers;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace PersonalFinanceTracker.Views
{
    public partial class AddEditTransactionWindow : Window
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly TransactionRepository _transactionRepository;
        private Transaction? _transaction;
        private bool _isEditMode;

        public bool IsSaved { get; private set; }

        // Constructor for ADD mode
        public AddEditTransactionWindow()
        {
            InitializeComponent();
            _categoryRepository = new CategoryRepository();
            _transactionRepository = new TransactionRepository();
            _isEditMode = false;

            // Add closing event handler
            Closing += Window_Closing;

            DatePicker.SelectedDate = DateTime.Now;
            LoadCategories("Income");
        }
        private void Input_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            MarkAsChanged();
        }
        private bool _hasChanges = false;

        // Track changes
        private void MarkAsChanged()
        {
            _hasChanges = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsSaved && _hasChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to close?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        // Constructor for EDIT mode
        public AddEditTransactionWindow(Transaction transaction) : this()
        {
            _transaction = transaction;
            _isEditMode = true;
            Title = "Edit Transaction";

            // Populate fields
            DatePicker.SelectedDate = transaction.Date;
            DescriptionTextBox.Text = transaction.Description;
            AmountTextBox.Text = transaction.Amount.ToString();

            // Set Type
            TypeComboBox.SelectedIndex = transaction.Type == "Income" ? 0 : 1;

            // Load and select category
            LoadCategories(transaction.Type);
            var categoryToSelect = CategoryComboBox.Items.Cast<Category>()
                .FirstOrDefault(c => c.Name == transaction.Category);
            if (categoryToSelect != null)
            {
                CategoryComboBox.SelectedItem = categoryToSelect;
            }
        }

        private void LoadCategories(string type)
        {
            // Safety check
            if (CategoryComboBox == null) return;

            CategoryComboBox.Items.Clear();
            var categories = _categoryRepository.GetByType(type);
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }
            if (CategoryComboBox.Items.Count > 0)
            {
                CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Safety check
            if (CategoryComboBox == null) return;

            if (TypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedType = selectedItem.Content.ToString() ?? "Income";
                LoadCategories(selectedType);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sanitize inputs
            DescriptionTextBox.Text = ValidationHelper.SanitizeInput(DescriptionTextBox.Text);
            AmountTextBox.Text = ValidationHelper.SanitizeInput(AmountTextBox.Text);

            // Validation with specific error messages
            if (!ValidationHelper.IsNotEmpty(DescriptionTextBox.Text))
            {
                MessageBox.Show("Please enter a description for this transaction.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                DescriptionTextBox.Focus();
                return;
            }

            if (DescriptionTextBox.Text.Length > 100)
            {
                MessageBox.Show("Description must be less than 100 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                DescriptionTextBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidDecimal(AmountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Please enter a valid positive amount.\nExample: 50.00",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return;
            }

            if (!ValidationHelper.IsValidAmount(amount))
            {
                MessageBox.Show("Amount must be between $0.01 and $999,999,999.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return;
            }

            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                DatePicker.Focus();
                return;
            }

            // Check if date is not in the future
            if (DatePicker.SelectedDate.Value > DateTime.Now)
            {
                var result = MessageBox.Show(
                    "The selected date is in the future. Do you want to continue?",
                    "Future Date",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    DatePicker.Focus();
                    return;
                }
            }

            try
            {
                var selectedCategory = (Category)CategoryComboBox.SelectedItem;
                var selectedType = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString() ?? "Income";

                if (_isEditMode && _transaction != null)
                {
                    // UPDATE existing transaction
                    _transaction.Date = DatePicker.SelectedDate.Value;
                    _transaction.Description = DescriptionTextBox.Text;
                    _transaction.Category = selectedCategory.Name;
                    _transaction.Type = selectedType;
                    _transaction.Amount = amount;

                    _transactionRepository.Update(_transaction);
                }
                else
                {
                    // ADD new transaction
                    var newTransaction = new Transaction
                    {
                        Date = DatePicker.SelectedDate.Value,
                        Description = DescriptionTextBox.Text,
                        Category = selectedCategory.Name,
                        Type = selectedType,
                        Amount = amount
                    };

                    _transactionRepository.Add(newTransaction);
                }

                IsSaved = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while saving the transaction:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers and decimal point
            Regex regex = new Regex(@"^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            Close();
        }
    }
}