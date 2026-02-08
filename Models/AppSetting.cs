namespace PersonalFinanceTracker.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        public string SelectedCurrency { get; set; }
        public string BaseCurrency { get; set; } // The currency transactions are stored in

        public AppSettings()
        {
            SelectedCurrency = "USD";
            BaseCurrency = "USD"; // All amounts stored in database are in USD by default
        }
    }
}