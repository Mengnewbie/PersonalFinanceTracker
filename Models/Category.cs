namespace PersonalFinanceTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "Income" or "Expense"
        public string Icon { get; set; }
        public string Color { get; set; }

        public Category()
        {
            Name = string.Empty;
            Type = string.Empty;
            Icon = string.Empty;
            Color = "#3498DB";
        }
    }
}