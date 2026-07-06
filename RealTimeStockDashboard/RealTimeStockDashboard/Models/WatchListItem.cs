using System;

namespace WatchListService
{
    public class WatchlistItem
    {
        public string StockSymbol { get; set; }
        public string StockName { get; set; }
        public DateTime AddedAt { get; set; }
    }
}