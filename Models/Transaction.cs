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
        public string Currency { get; set; }

        // Computed display property for dashboard/reports
        public string DisplayAmount
        {
            get
            {
                var prefix = Type == "Income" ? "+" : "-";
                return $"{prefix}{Amount:N2} {Currency}";
            }
        }

        public Transaction()
        {
            Description = string.Empty;
            Category = string.Empty;
            Type = "Income"; // FIX: Was assigned twice (string.Empty then "Income")
            Currency = "USD";
            Date = DateTime.Now;
        }
    }
}