using System.Windows;

namespace PersonalFinanceTracker.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = this
            };
            settingsWindow.ShowDialog();

            if (settingsWindow.IsSaved)
            {
                // Get the current ViewModel type
                var mainViewModel = (ViewModels.MainViewModel)DataContext;
                var currentType = mainViewModel.CurrentViewModel?.GetType();

                // Recreate the entire MainViewModel to force complete refresh
                DataContext = null;  // Clear first
                DataContext = new ViewModels.MainViewModel();

                // Navigate back to the same page the user was on
                mainViewModel = (ViewModels.MainViewModel)DataContext;

                if (currentType == typeof(ViewModels.DashboardViewModel))
                    mainViewModel.NavigateToDashboardCommand.Execute(null);
                else if (currentType == typeof(ViewModels.TransactionsViewModel))
                    mainViewModel.NavigateToTransactionsCommand.Execute(null);
                else if (currentType == typeof(ViewModels.CategoriesViewModel))
                    mainViewModel.NavigateToCategoriesCommand.Execute(null);
                else if (currentType == typeof(ViewModels.ReportsViewModel))
                    mainViewModel.NavigateToReportsCommand.Execute(null);
                else if (currentType == typeof(ViewModels.BudgetViewModel))
                    mainViewModel.NavigateToBudgetCommand.Execute(null);
            }
        }
    }
}