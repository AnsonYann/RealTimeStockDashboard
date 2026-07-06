using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;             // <-- FIX 1: This removes the red line under JObject
using RealTimeStockDashboard.Models;
using RealTimeStockDashboard.Utilities;

namespace RealTimeStockDashboard.Services
{
    internal class CurencyService
    {
        public async Task<CurrencyRate> GetExchangeRateAsync(string from, string to)

        {
            string url =

                $"https://api.frankfurter.app/latest?from={from}&to={to}";

            string json = await ApiHelper.GetAsync(url);

            JObject data = JObject.Parse(json);

            double rate = (double)data["rates"][to];

            return new CurrencyRate

            {
                FromCurrency = from,

                ToCurrency = to,

                Rate = (decimal)rate,

                Date = DateTime.Now.ToString("yyyy-MM-dd")

            };
        }
    }
}
