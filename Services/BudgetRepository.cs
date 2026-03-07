using System.Collections.Generic;
using System.Data.SQLite;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class BudgetRepository
    {
        // CREATE
        public void Add(Budget budget)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                INSERT INTO Budgets (Category, BudgetAmount, Period, Currency)
                VALUES (@Category, @BudgetAmount, @Period, @Currency);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Category", budget.Category);
            command.Parameters.AddWithValue("@BudgetAmount", budget.BudgetAmount);
            command.Parameters.AddWithValue("@Period", budget.Period);
            command.Parameters.AddWithValue("@Currency", budget.Currency);

            command.ExecuteNonQuery();
        }

        // READ
        public List<Budget> GetAll()
        {
            var budgets = new List<Budget>();

            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            // FIX: Select columns explicitly instead of SELECT *
            // This avoids ordinal mismatches and is more maintainable
            string query = "SELECT Id, Category, BudgetAmount, Period, Currency FROM Budgets ORDER BY Category;";

            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var budget = new Budget
                {
                    Id = reader.GetInt32(0),
                    Category = reader.GetString(1),
                    BudgetAmount = reader.GetDecimal(2),
                    Period = reader.GetString(3),
                    // FIX: Use IsDBNull instead of try/catch for Currency column
                    Currency = !reader.IsDBNull(4) ? reader.GetString(4) : "USD"
                };

                budgets.Add(budget);
            }

            return budgets;
        }

        // READ by category
        public Budget? GetByCategory(string category)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT Id, Category, BudgetAmount, Period, Currency FROM Budgets WHERE Category = @Category;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Category", category);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Budget
                {
                    Id = reader.GetInt32(0),
                    Category = reader.GetString(1),
                    BudgetAmount = reader.GetDecimal(2),
                    Period = reader.GetString(3),
                    Currency = !reader.IsDBNull(4) ? reader.GetString(4) : "USD"
                };
            }

            return null;
        }

        // UPDATE
        public void Update(Budget budget)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                UPDATE Budgets 
                SET Category = @Category, 
                    BudgetAmount = @BudgetAmount, 
                    Period = @Period,
                    Currency = @Currency
                WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", budget.Id);
            command.Parameters.AddWithValue("@Category", budget.Category);
            command.Parameters.AddWithValue("@BudgetAmount", budget.BudgetAmount);
            command.Parameters.AddWithValue("@Period", budget.Period);
            command.Parameters.AddWithValue("@Currency", budget.Currency);

            command.ExecuteNonQuery();
        }

        // DELETE
        public void Delete(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "DELETE FROM Budgets WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }
    }
}