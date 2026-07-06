using System;
using System.Collections.Generic;
using System.Text;
using RealTimeStockDashboard.Utilities;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RealTimeStockDashboard.Services
{
    internal class StockApiServices
    {
        private const string API_KEY = "TJ685SKTMMZNR1AW";

        public async Task<(string symbol, double price, double change)> GetStockAsync(string stockSymbol)
        {
            try
            {
                string url =
                    $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={stockSymbol}&apikey={API_KEY}";
                string json = await ApiHelper.GetAsync(url);

                JObject data = JObject.Parse(json);

                var quote = data["Global Quote"];

                if (quote == null)
                {
                    throw new Exception("Stock not found.");
                }

                string symbol = quote["01. symbol"]?.ToString();

                double price = double.Parse(
                    quote["05. price"]?.ToString() ?? "0"
                );

                double change = double.Parse(
                    quote["09. change"]?.ToString() ?? "0"
                );

                return (symbol, price, change);
            }
            catch (Exception ex)
            {
                throw new Exception($"API Error: {ex.Message}");
            }

        }
    }
}
