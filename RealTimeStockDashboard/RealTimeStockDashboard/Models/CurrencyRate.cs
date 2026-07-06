namespace RealTimeStockDashboard.Models
{
    public class CurrencyRate
    {
        public string FromCurrency { get; set; } = string.Empty;

        public string ToCurrency { get; set; } = string.Empty;

        public decimal Rate { get; set; }

        public string Date { get; set; } = string.Empty;
    }
}