using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WatchListService
{
    // ===  臨時 Model (暫時頂替 Member 5 的工作) ===
    //public class WatchlistItem
    //{
    //    public string StockSymbol { get; set; } // 股票代號 (例如: AAPL, 2330)
    //    public string StockName { get; set; }   // 股票名稱 (例如: Apple, 台積電)
    //    public DateTime AddedAt { get; set; }   // 加入最愛的時間
    //}

    // === 核心服務類別 ===
    public class WatchlistService
    {
        // 自動定位到程式執行目錄底下的 Data/watchlist.json
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "watchlist.json");

        public WatchlistService()
        {
            InitializeFile();
        }

        private void InitializeFile()
        {
            string directory = Path.GetDirectoryName(_filePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        // 功能 1：載入最愛股票列表
        public List<WatchlistItem> LoadWatchlist()
        {
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                List<WatchlistItem> list = JsonSerializer.Deserialize<List<WatchlistItem>>(jsonString);
                return list ?? new List<WatchlistItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取最愛清單失敗: {ex.Message}");
                return new List<WatchlistItem>();
            }
        }

        // 輔助功能：儲存最愛股票列表
        private void SaveWatchlist(List<WatchlistItem> list)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(list, options);
            File.WriteAllText(_filePath, jsonString);
        }

        // 功能 2：新增股票到最愛
        public bool AddToWatchlist(WatchlistItem item)
        {
            var currentList = LoadWatchlist();

            bool isExist = currentList.Exists(x => x.StockSymbol.Equals(item.StockSymbol, StringComparison.OrdinalIgnoreCase));

            if (isExist)
            {
                return false; // 重複了，不加入
            }

            item.AddedAt = DateTime.Now;
            currentList.Add(item);
            SaveWatchlist(currentList);
            return true;
        }

        // 功能 3：從最愛移除股票
        public bool RemoveFromWatchlist(string stockSymbol)
        {
            var currentList = LoadWatchlist();
            var itemToRemove = currentList.Find(x => x.StockSymbol.Equals(stockSymbol, StringComparison.OrdinalIgnoreCase));

            if (itemToRemove != null)
            {
                currentList.Remove(itemToRemove);
                SaveWatchlist(currentList);
                return true;
            }

            return false;
        }
    }
}