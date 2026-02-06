using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class CategoriesViewModel : BaseViewModel
    {
        private readonly CategoryRepository _categoryRepository;
        private ObservableCollection<Category> _categories;
        private Category? _selectedCategory;

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public CategoriesViewModel()
        {
            _categoryRepository = new CategoryRepository();
            _categories = new ObservableCollection<Category>();

            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEditDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteEditDelete);

            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            var categories = _categoryRepository.GetAll();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private void ExecuteAdd(object? parameter)
        {
            var dialog = new Views.AddEditCategoryWindow();
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadCategories();
                MessageBox.Show("Category added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteEdit(object? parameter)
        {
            if (SelectedCategory == null) return;

            var dialog = new Views.AddEditCategoryWindow(SelectedCategory);
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadCategories();
                MessageBox.Show("Category updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteDelete(object? parameter)
        {
            if (SelectedCategory == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete category '{SelectedCategory.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _categoryRepository.Delete(SelectedCategory.Id);
                LoadCategories(); // Refresh the list
                MessageBox.Show("Category deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanExecuteEditDelete(object? parameter)
        {
            return SelectedCategory != null;
        }

        public void RefreshCategories()
        {
            LoadCategories();
        }
    }
}