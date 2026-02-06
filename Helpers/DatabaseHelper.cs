using System;
using System.Data.SQLite;
using System.IO;

namespace PersonalFinanceTracker.Helpers
{
    public class DatabaseHelper
    {
        private static readonly string DatabaseFileName = "FinanceTracker.db";
        private static readonly string DatabasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PersonalFinanceTracker",
            DatabaseFileName
        );

        public static string ConnectionString => $"Data Source={DatabasePath};Version=3;";

        public static void InitializeDatabase()
        {
            // Create directory if it doesn't exist
            string? directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create database file if it doesn't exist
            if(!File.Exists(DatabasePath))
{
                SQLiteConnection.CreateFile(DatabasePath);
                CreateTables();
                InsertDefaultCategories();
            }

        }

        private static void CreateTables()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            string createCategoriesTable = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Icon TEXT,
                    Color TEXT
                );";

            string createTransactionsTable = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Amount REAL NOT NULL
                );";

            string createBudgetsTable = @"
                CREATE TABLE IF NOT EXISTS Budgets (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT NOT NULL,
                    BudgetAmount REAL NOT NULL,
                    Period TEXT NOT NULL
                );";

            using (var command = new SQLiteCommand(createCategoriesTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createTransactionsTable, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createBudgetsTable, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private static void InsertDefaultCategories()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            string insertQuery = @"
                INSERT INTO Categories (Name, Type, Icon, Color) 
                VALUES (@Name, @Type, @Icon, @Color);";

            // Default Expense Categories
            var expenseCategories = new[]
            {
                new { Name = "Food & Dining", Type = "Expense", Icon = "🍔", Color = "#E74C3C" },
                new { Name = "Transportation", Type = "Expense", Icon = "🚗", Color = "#9B59B6" },
                new { Name = "Utilities", Type = "Expense", Icon = "💡", Color = "#F39C12" },
                new { Name = "Shopping", Type = "Expense", Icon = "🛒", Color = "#E67E22" },
                new { Name = "Entertainment", Type = "Expense", Icon = "🎮", Color = "#1ABC9C" },
                new { Name = "Healthcare", Type = "Expense", Icon = "🏥", Color = "#16A085" },
                new { Name = "Education", Type = "Expense", Icon = "📚", Color = "#2980B9" },
                new { Name = "Other Expenses", Type = "Expense", Icon = "📦", Color = "#95A5A6" }
            };

            // Default Income Categories
            var incomeCategories = new[]
            {
                new { Name = "Salary", Type = "Income", Icon = "💰", Color = "#27AE60" },
                new { Name = "Freelance", Type = "Income", Icon = "💼", Color = "#3498DB" },
                new { Name = "Investment", Type = "Income", Icon = "📈", Color = "#2ECC71" },
                new { Name = "Other Income", Type = "Income", Icon = "💵", Color = "#1ABC9C" }
            };

            // Insert all categories
            foreach (var category in expenseCategories)
            {
                using var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Name", category.Name);
                command.Parameters.AddWithValue("@Type", category.Type);
                command.Parameters.AddWithValue("@Icon", category.Icon);
                command.Parameters.AddWithValue("@Color", category.Color);
                command.ExecuteNonQuery();
            }

            foreach (var category in incomeCategories)
            {
                using var command = new SQLiteCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@Name", category.Name);
                command.Parameters.AddWithValue("@Type", category.Type);
                command.Parameters.AddWithValue("@Icon", category.Icon);
                command.Parameters.AddWithValue("@Color", category.Color);
                command.ExecuteNonQuery();
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}