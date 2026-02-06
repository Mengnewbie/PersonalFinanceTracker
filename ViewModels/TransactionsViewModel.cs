using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;

namespace PersonalFinanceTracker.ViewModels
{
    public class TransactionsViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private ObservableCollection<Transaction> _transactions;
        private Transaction? _selectedTransaction;

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        public Transaction? SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public TransactionsViewModel()
        {
            _transactionRepository = new TransactionRepository();
            _transactions = new ObservableCollection<Transaction>();

            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEditDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteEditDelete);

            LoadTransactions();
        }

        private void LoadTransactions()
        {
            Transactions.Clear();
            var transactions = _transactionRepository.GetAll();
            foreach (var transaction in transactions)
            {
                Transactions.Add(transaction);
            }
        }

        private void ExecuteAdd(object? parameter)
        {
            var dialog = new Views.AddEditTransactionWindow();
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadTransactions();
                MessageBox.Show("Transaction added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteEdit(object? parameter)
        {
            if (SelectedTransaction == null) return;

            var dialog = new Views.AddEditTransactionWindow(SelectedTransaction);
            dialog.ShowDialog();

            if (dialog.IsSaved)
            {
                LoadTransactions();
                MessageBox.Show("Transaction updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteDelete(object? parameter)
        {
            if (SelectedTransaction == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedTransaction.Description}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _transactionRepository.Delete(SelectedTransaction.Id);
                LoadTransactions(); // Refresh the list
                MessageBox.Show("Transaction deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanExecuteEditDelete(object? parameter)
        {
            return SelectedTransaction != null;
        }

        public void RefreshTransactions()
        {
            LoadTransactions();
        }
    }
}