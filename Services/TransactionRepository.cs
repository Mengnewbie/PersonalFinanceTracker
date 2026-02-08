using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class TransactionRepository
    {
        private readonly CurrencyService _currencyService;

        public TransactionRepository()
        {
            _currencyService = new CurrencyService();
        }

        // CREATE
        public void Add(Transaction transaction)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                INSERT INTO Transactions (Date, Description, Category, Type, Amount, Currency)
                VALUES (@Date, @Description, @Category, @Type, @Amount, @Currency);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Description", transaction.Description);
            command.Parameters.AddWithValue("@Category", transaction.Category);
            command.Parameters.AddWithValue("@Type", transaction.Type);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@Currency", transaction.Currency);

            command.ExecuteNonQuery();
        }

        // READ
        public List<Transaction> GetAll()
        {
            var transactions = new List<Transaction>();

            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Transactions ORDER BY Date DESC;";

            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var transaction = new Transaction
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    Description = reader.GetString(2),
                    Category = reader.GetString(3),
                    Type = reader.GetString(4),
                    Amount = reader.GetDecimal(5)
                };

                // Try to get Currency column (might not exist in old databases)
                try
                {
                    transaction.Currency = reader.GetString(6);
                }
                catch
                {
                    transaction.Currency = "USD"; // Default for old records
                }

                transactions.Add(transaction);
            }

            return transactions;
        }

        // UPDATE
        public void Update(Transaction transaction)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                UPDATE Transactions 
                SET Date = @Date, 
                    Description = @Description, 
                    Category = @Category, 
                    Type = @Type, 
                    Amount = @Amount,
                    Currency = @Currency
                WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", transaction.Id);
            command.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Description", transaction.Description);
            command.Parameters.AddWithValue("@Category", transaction.Category);
            command.Parameters.AddWithValue("@Type", transaction.Type);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);
            command.Parameters.AddWithValue("@Currency", transaction.Currency);

            command.ExecuteNonQuery();
        }

        // DELETE
        public void Delete(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "DELETE FROM Transactions WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        // Get total income (converted to base currency USD)
        public decimal GetTotalIncome()
        {
            var transactions = GetAll();
            decimal total = 0;

            foreach (var t in transactions)
            {
                if (t.Type == "Income")
                {
                    // Convert to USD for calculation
                    total += _currencyService.ConvertToUSD(t.Amount, t.Currency);
                }
            }

            return total;
        }

        // Get total expenses (converted to base currency USD)
        public decimal GetTotalExpenses()
        {
            var transactions = GetAll();
            decimal total = 0;

            foreach (var t in transactions)
            {
                if (t.Type == "Expense")
                {
                    // Convert to USD for calculation
                    total += _currencyService.ConvertToUSD(t.Amount, t.Currency);
                }
            }

            return total;
        }
    }
}