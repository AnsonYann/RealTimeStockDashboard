using System;
using System.IO;
using System.Text.Json;

namespace RealTimeStockDashboard.Utilities
{
    public static class JsonHelper
    {
        // JSON 設定
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

        // 物件 JSON字串
        public static string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, Options);
        }

        // JSON字串 物件
        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }

        // 儲存JSON檔案（自動建立資料夾）
        public static void SaveToFile<T>(string filePath, T data)
        {
            try
            {
                string json = Serialize(data);

                string? directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) &&
                    !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save failed: {ex.Message}");
            }
        }

        // 從JSON檔案讀取
        // T必須可建立空物件（避免讀取失敗時回傳null）
        public static T LoadFromFile<T>(string filePath)
            where T : new()
        {
            try
            {
                // 檔案不存在 回傳空物件
                if (!File.Exists(filePath))
                    return new T();

                string json = File.ReadAllText(filePath);

                var result = Deserialize<T>(json);

                // JSON 解析失敗 回傳空物件
                return result != null ? result : new T();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load failed: {ex.Message}");

                // 發生錯誤 回傳空物件避免程式閃退
                return new T();
            }
        }

        // API JSON字串 物件
        public static T? ParseJson<T>(string jsonString)
        {
            return Deserialize<T>(jsonString);
        }
    }
}