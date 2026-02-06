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

        public Transaction()
        {
            Description = string.Empty;
            Category = string.Empty;
            Type = string.Empty;
        }
    }
}