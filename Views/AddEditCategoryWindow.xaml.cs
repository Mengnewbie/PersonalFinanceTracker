using System.Windows;
using System.Windows.Controls;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.Views
{
    public partial class AddEditCategoryWindow : Window
    {
        private readonly CategoryRepository _categoryRepository;
        private Category? _category;
        private bool _isEditMode;

        public bool IsSaved { get; private set; }

        // Constructor for ADD mode
        public AddEditCategoryWindow()
        {
            InitializeComponent();
            _categoryRepository = new CategoryRepository();
            _isEditMode = false;
        }

        // Constructor for EDIT mode
        public AddEditCategoryWindow(Category category) : this()
        {
            _category = category;
            _isEditMode = true;
            Title = "Edit Category";

            // Populate fields
            NameTextBox.Text = category.Name;
            TypeComboBox.SelectedIndex = category.Type == "Income" ? 0 : 1;
            IconTextBox.Text = category.Icon;
            ColorTextBox.Text = category.Color;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sanitize inputs
            NameTextBox.Text = ValidationHelper.SanitizeInput(NameTextBox.Text);
            IconTextBox.Text = ValidationHelper.SanitizeInput(IconTextBox.Text);
            ColorTextBox.Text = ValidationHelper.SanitizeInput(ColorTextBox.Text);

            // Validation
            if (!ValidationHelper.IsNotEmpty(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a category name.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (NameTextBox.Text.Length > 50)
            {
                MessageBox.Show("Category name must be less than 50 characters.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            // Validate color hex
            if (!ValidationHelper.IsValidColorHex(ColorTextBox.Text))
            {
                MessageBox.Show("Please enter a valid hex color.\nExample: #3498DB",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                ColorTextBox.Focus();
                return;
            }

            try
            {
                var selectedType = ((ComboBoxItem)TypeComboBox.SelectedItem).Content.ToString() ?? "Income";

                if (_isEditMode && _category != null)
                {
                    // UPDATE existing category
                    _category.Name = NameTextBox.Text;
                    _category.Type = selectedType;
                    _category.Icon = IconTextBox.Text;
                    _category.Color = ColorTextBox.Text;

                    _categoryRepository.Update(_category);
                }
                else
                {
                    // Check for duplicate category name
                    var existingCategories = _categoryRepository.GetAll();
                    if (existingCategories.Any(c => c.Name.Equals(NameTextBox.Text, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show(
                            $"A category named '{NameTextBox.Text}' already exists.",
                            "Duplicate Category",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        NameTextBox.Focus();
                        return;
                    }

                    // ADD new category
                    var newCategory = new Category
                    {
                        Name = NameTextBox.Text,
                        Type = selectedType,
                        Icon = IconTextBox.Text,
                        Color = ColorTextBox.Text
                    };

                    _categoryRepository.Add(newCategory);
                }

                IsSaved = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while saving the category:\n{ex.Message}",
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