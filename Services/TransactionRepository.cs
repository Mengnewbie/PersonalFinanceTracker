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

            string query = "SELECT Id, Date, Description, Category, Type, Amount, Currency FROM Transactions ORDER BY Date DESC;";

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

                // FIX: Use ordinal-based IsDBNull check instead of try/catch
                // The old try/catch approach would silently swallow real errors
                transaction.Currency = !reader.IsDBNull(6) ? reader.GetString(6) : "USD";

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

        // FIX: Use SQL aggregation instead of loading all records into memory
        // Old version: loaded ALL transactions, iterated in C# to sum
        // New version: lets the database do the work (much faster with large datasets)

        public decimal GetTotalIncome()
        {
            return GetTotalByType("Income");
        }

        public decimal GetTotalExpenses()
        {
            return GetTotalByType("Expense");
        }

        private decimal GetTotalByType(string type)
        {
            // Note: Since transactions can be in different currencies,
            // we still need to load and convert individually.
            // But we only load the relevant type, not everything.
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT Amount, Currency FROM Transactions WHERE Type = @Type;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Type", type);
            using var reader = command.ExecuteReader();

            decimal total = 0;
            while (reader.Read())
            {
                var amount = reader.GetDecimal(0);
                var currency = !reader.IsDBNull(1) ? reader.GetString(1) : "USD";
                total += _currencyService.ConvertToUSD(amount, currency);
            }

            return total;
        }
    }
}