using System.Data.SQLite;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class SettingsRepository
    {
        public AppSettings GetSettings()
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            // First, try to get settings with BaseCurrency column
            string query = "SELECT * FROM Settings LIMIT 1;";

            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var settings = new AppSettings
                {
                    Id = reader.GetInt32(0),
                    SelectedCurrency = reader.GetString(1)
                };

                // Check if BaseCurrency column exists
                try
                {
                    settings.BaseCurrency = reader.GetString(2);
                }
                catch
                {
                    settings.BaseCurrency = "USD"; // Default if column doesn't exist yet
                }

                return settings;
            }

            // Return default if not found
            return new AppSettings { Id = 1, SelectedCurrency = "USD", BaseCurrency = "USD" };
        }

        public void UpdateCurrency(string currencyCode)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                UPDATE Settings 
                SET SelectedCurrency = @Currency 
                WHERE Id = 1;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Currency", currencyCode);
            command.ExecuteNonQuery();
        }

        public void UpdateBaseCurrency(string currencyCode)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                UPDATE Settings 
                SET BaseCurrency = @Currency 
                WHERE Id = 1;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Currency", currencyCode);
            command.ExecuteNonQuery();
        }
    }
}