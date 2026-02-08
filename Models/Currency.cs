namespace PersonalFinanceTracker.Models
{
    public class Currency
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public decimal ExchangeRateToUSD { get; set; } // How many of this currency = 1 USD

        public Currency()
        {
            Code = "USD";
            Symbol = "$";
            Name = "US Dollar";
            ExchangeRateToUSD = 1.0m;
        }

        public Currency(string code, string symbol, string name, decimal exchangeRateToUSD)
        {
            Code = code;
            Symbol = symbol;
            Name = name;
            ExchangeRateToUSD = exchangeRateToUSD;
        }

        public string DisplayName => $"{Symbol} {Code} - {Name}";
    }
}