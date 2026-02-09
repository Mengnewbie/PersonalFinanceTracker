namespace PersonalFinanceTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public decimal BudgetAmount { get; set; }
        public string Period { get; set; } // "Monthly", "Weekly", "Yearly"

        public string Currency { get; set; } // NEW: Currency code

        public Budget()
        {
            Category = string.Empty;
            Period = "Monthly";
            Currency = "USD"; // Default to USD
        }
    }
}
