using System.Collections.Generic;
using System.Data.SQLite;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class BudgetRepository
    {
        // CREATE - Add new budget
        public void Add(Budget budget)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                INSERT INTO Budgets (Category, BudgetAmount, Period)
                VALUES (@Category, @BudgetAmount, @Period);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Category", budget.Category);
            command.Parameters.AddWithValue("@BudgetAmount", budget.BudgetAmount);
            command.Parameters.AddWithValue("@Period", budget.Period);

            command.ExecuteNonQuery();
        }

        // READ - Get all budgets
        public List<Budget> GetAll()
        {
            var budgets = new List<Budget>();

            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Budgets ORDER BY Category;";

            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                budgets.Add(new Budget
                {
                    Id = reader.GetInt32(0),
                    Category = reader.GetString(1),
                    BudgetAmount = reader.GetDecimal(2),
                    Period = reader.GetString(3)
                });
            }

            return budgets;
        }

        // READ - Get budget by category
        public Budget? GetByCategory(string category)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Budgets WHERE Category = @Category;";

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
                    Period = reader.GetString(3)
                };
            }

            return null;
        }

        // UPDATE - Edit existing budget
        public void Update(Budget budget)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                UPDATE Budgets 
                SET Category = @Category, 
                    BudgetAmount = @BudgetAmount, 
                    Period = @Period
                WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", budget.Id);
            command.Parameters.AddWithValue("@Category", budget.Category);
            command.Parameters.AddWithValue("@BudgetAmount", budget.BudgetAmount);
            command.Parameters.AddWithValue("@Period", budget.Period);

            command.ExecuteNonQuery();
        }

        // DELETE - Remove budget
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