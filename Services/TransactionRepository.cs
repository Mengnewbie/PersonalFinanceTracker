using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class TransactionRepository
    {
        // CREATE - Add new transaction
        public void Add(Transaction transaction)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                INSERT INTO Transactions (Date, Description, Category, Type, Amount)
                VALUES (@Date, @Description, @Category, @Type, @Amount);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Description", transaction.Description);
            command.Parameters.AddWithValue("@Category", transaction.Category);
            command.Parameters.AddWithValue("@Type", transaction.Type);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);

            command.ExecuteNonQuery();
        }

        // READ - Get all transactions
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
                transactions.Add(new Transaction
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    Description = reader.GetString(2),
                    Category = reader.GetString(3),
                    Type = reader.GetString(4),
                    Amount = reader.GetDecimal(5)
                });
            }

            return transactions;
        }

        // READ - Get transaction by ID
        public Transaction? GetById(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Transactions WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Transaction
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    Description = reader.GetString(2),
                    Category = reader.GetString(3),
                    Type = reader.GetString(4),
                    Amount = reader.GetDecimal(5)
                };
            }

            return null;
        }

        // UPDATE - Edit existing transaction
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
                    Amount = @Amount
                WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", transaction.Id);
            command.Parameters.AddWithValue("@Date", transaction.Date.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@Description", transaction.Description);
            command.Parameters.AddWithValue("@Category", transaction.Category);
            command.Parameters.AddWithValue("@Type", transaction.Type);
            command.Parameters.AddWithValue("@Amount", transaction.Amount);

            command.ExecuteNonQuery();
        }

        // DELETE - Remove transaction
        public void Delete(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "DELETE FROM Transactions WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        // Get total income
        public decimal GetTotalIncome()
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT COALESCE(SUM(Amount), 0) FROM Transactions WHERE Type = 'Income';";

            using var command = new SQLiteCommand(query, connection);
            var result = command.ExecuteScalar();

            return Convert.ToDecimal(result);
        }

        // Get total expenses
        public decimal GetTotalExpenses()
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT COALESCE(SUM(Amount), 0) FROM Transactions WHERE Type = 'Expense';";

            using var command = new SQLiteCommand(query, connection);
            var result = command.ExecuteScalar();

            return Convert.ToDecimal(result);
        }
    }
}