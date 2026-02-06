using System.Collections.Generic;
using System.Data.SQLite;
using PersonalFinanceTracker.Helpers;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class CategoryRepository
    {
        // CREATE - Add new category
        public void Add(Category category)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                INSERT INTO Categories (Name, Type, Icon, Color)
                VALUES (@Name, @Type, @Icon, @Color);";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Type", category.Type);
            command.Parameters.AddWithValue("@Icon", category.Icon);
            command.Parameters.AddWithValue("@Color", category.Color);

            command.ExecuteNonQuery();
        }

        // READ - Get all categories
        public List<Category> GetAll()
        {
            var categories = new List<Category>();

            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Categories ORDER BY Type, Name;";

            using var command = new SQLiteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Icon = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Color = reader.IsDBNull(4) ? "#3498DB" : reader.GetString(4)
                });
            }

            return categories;
        }

        // READ - Get category by ID
        public Category? GetById(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Categories WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Icon = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Color = reader.IsDBNull(4) ? "#3498DB" : reader.GetString(4)
                };
            }

            return null;
        }

        // UPDATE - Edit existing category
        public void Update(Category category)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = @"
                UPDATE Categories 
                SET Name = @Name, 
                    Type = @Type, 
                    Icon = @Icon, 
                    Color = @Color
                WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", category.Id);
            command.Parameters.AddWithValue("@Name", category.Name);
            command.Parameters.AddWithValue("@Type", category.Type);
            command.Parameters.AddWithValue("@Icon", category.Icon);
            command.Parameters.AddWithValue("@Color", category.Color);

            command.ExecuteNonQuery();
        }

        // DELETE - Remove category
        public void Delete(int id)
        {
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "DELETE FROM Categories WHERE Id = @Id;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        // Get categories by type (Income or Expense)
        public List<Category> GetByType(string type)
        {
            var categories = new List<Category>();

            using var connection = DatabaseHelper.GetConnection();
            connection.Open();

            string query = "SELECT * FROM Categories WHERE Type = @Type ORDER BY Name;";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@Type", type);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Type = reader.GetString(2),
                    Icon = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Color = reader.IsDBNull(4) ? "#3498DB" : reader.GetString(4)
                });
            }

            return categories;
        }
    }
}