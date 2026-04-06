using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OrdersDownloader.Models;

namespace OrdersDownloader.Services
{
    public class JsonSaver
    {
        public async Task SaveRawJsonAsync(List<Order> orders)
        {
            var fileName = $"orders_raw_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            var json = JsonSerializer.Serialize(orders, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(fileName, json);

            Console.WriteLine($"Saved RAW JSON: {fileName}");
        }
    }
}