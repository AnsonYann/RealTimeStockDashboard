using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace RealTimeStockDashboard.Utilities
{
    internal class ApiHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetAsync(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}