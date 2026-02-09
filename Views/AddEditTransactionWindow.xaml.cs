using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Views
{
    public partial class AddEditTransactionWindow : Window
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly CurrencyService _currencyService;
        private readonly SettingsRepository _settingsRepository;
        private Transaction? _transaction;
        private bool _isEditMode;
        private bool _hasChanges = false;

        public bool IsSaved { get; private set; }

        // Constructor for ADD mode
        public AddEditTransactionWindow()
        {
            InitializeComponent();
            _categoryRepository = new CategoryRepository();
            _transactionRepository = new TransactionRepository();
            _currencyService = new CurrencyService();
            _settingsRepository = new SettingsRepository();
            _isEditMode = false;

            Closing += Window_Closing;
        }

        // Constructor for EDIT mode
        public AddEditTransactionWindow(Transaction transaction) : this()
        {
            _transaction = transaction;
            _isEditMode = true;
            Title = "Edit Transaction";

            // Pre-fill fields
            DatePicker.SelectedDate = transaction.Date;
            DescriptionTextBox.Text = transaction.Description;
            AmountTextBox.Text = transaction.Amount.ToString();

            // Set Type
            TypeComboBox.SelectedIndex = transaction.Type == "Income" ? 0 : 1;

            // Currency will be set in Window_Loaded
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrencies();
            LoadCategories(TypeComboBox.SelectedIndex == 0 ? "Income" : "Expense");

            if (_isEditMode && _transaction != null)
            {
                // Select the transaction's currency
                var transactionCurrency = _currencyService.GetCurrency(_transaction.Currency);
                CurrencyComboBox.SelectedItem = transactionCurrency;

                // Select category
                var categoryToSelect = CategoryComboBox.Items.Cast<Category>()
                    .FirstOrDefault(c => c.Name == _transaction.Category);
                if (categoryToSelect != null)
                {
                    CategoryComboBox.SelectedItem = categoryToSelect;
                }
            }
            else
            {
                // For new transactions, default to user's preferred currency
                var settings = _settingsRepository.GetSettings();
                var defaultCurrency = _currencyService.GetCurrency(settings.SelectedCurrency);
                CurrencyComboBox.SelectedItem = defaultCurrency;

                DatePicker.SelectedDate = DateTime.Now;
            }

            UpdateConversionInfo();
        }

        private void LoadCurrencies()
        {
            var currencies = _currencyService.GetAllCurrencies();
            CurrencyComboBox.ItemsSource = currencies;
        }

        private void LoadCategories(string type)
        {
            CategoryComboBox.Items.Clear();
            var categories = _categoryRepository.GetByType(type);
            foreach (var category in categories)
            {
                CategoryComboBox.Items.Add(category);
            }
            if (CategoryComboBox.Items.Count > 0 && !_isEditMode)
            {
                CategoryComboBox.SelectedIndex = 0;
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CategoryComboBox != null)
            {
                var selectedType = ((System.Windows.Controls.ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString();
                LoadCategories(selectedType ?? "Income");
            }
        }

        private void CurrencyComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateConversionInfo();
        }

        private void UpdateConversionInfo()
        {
            if (CurrencyComboBox.SelectedItem is Currency selectedCurrency &&
                decimal.TryParse(AmountTextBox.Text, out decimal amount) &&
                amount > 0)
            {
                var settings = _settingsRepository.GetSettings();

                if (selectedCurrency.Code != settings.SelectedCurrency)
                {
                    // Show conversion to user's display currency
                    var convertedAmount = _currencyService.Convert(amount, selectedCurrency.Code, settings.SelectedCurrency);
                    var displayCurrency = _currencyService.GetCurrency(settings.SelectedCurrency);

                    ConversionInfoText.Text = $"{selectedCurrency.Symbol}{amount:N2} {selectedCurrency.Code} = " +
                        $"{displayCurrency.Symbol}{convertedAmount:N2} {settings.SelectedCurrency}";
                }
                else
                {
                    ConversionInfoText.Text = $"Amount in {selectedCurrency.Code}";
                }
            }
            else if (CurrencyComboBox.SelectedItem is Currency currency)
            {
                ConversionInfoText.Text = $"Enter amount in {currency.Code}";
            }
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Input_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _hasChanges = true;
            UpdateConversionInfo();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sanitize inputs
            DescriptionTextBox.Text = ValidationHelper.SanitizeInput(DescriptionTextBox.Text);
            AmountTextBox.Text = ValidationHelper.SanitizeInput(AmountTextBox.Text);

            // Validation
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

            if (CurrencyComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a currency.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CurrencyComboBox.Focus();
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
                var selectedType = ((System.Windows.Controls.ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString() ?? "Income";
                var selectedCurrency = (Currency)CurrencyComboBox.SelectedItem;

                if (_isEditMode && _transaction != null)
                {
                    // UPDATE existing transaction
                    _transaction.Date = DatePicker.SelectedDate.Value;
                    _transaction.Description = DescriptionTextBox.Text;
                    _transaction.Category = selectedCategory.Name;
                    _transaction.Type = selectedType;
                    _transaction.Amount = amount;
                    _transaction.Currency = selectedCurrency.Code;

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
                        Amount = amount,
                        Currency = selectedCurrency.Code
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            Close();
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
    }
}