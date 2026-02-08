using System;

namespace PersonalFinanceTracker.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Type { get; set; } // "Income" or "Expense"
        public decimal Amount { get; set; }
        public string Currency { get; set; } // NEW: Currency code (USD, KHR, etc.)
        public Transaction()
        {
            Description = string.Empty;
            Category = string.Empty;
            Type = string.Empty;
            Type = "Income";
            Currency = "USD"; // Default to USD
        }
    }
}