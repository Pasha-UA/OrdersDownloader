using System.Net.Http.Headers;
using System.Text.Json;
using OrdersDownloader.Models;

public class PromApiClient
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly string _ordersUrl;

    public PromApiClient(HttpClient client, string baseUrl, string ordersUrl)
    {
        _client = client;
        _baseUrl = baseUrl;
        _ordersUrl = ordersUrl;
    }

    public async Task<string> GetOrdersAsync(string dateFrom, string dateTo, int limit = 100)
    {
        var url = $"{_ordersUrl}?date_from={dateFrom}&date_to={dateTo}&limit={limit}";
        return await _client.GetStringAsync(url);
    }

    public async Task<List<Order>> GetOrdersForPeriodAsync(int days)
    {
        var allOrders = new List<Order>();
        var loadedIds = new HashSet<long>(); // 🔥 защита от дублей

        var dateTo = DateTime.Now;
        var dateFrom = dateTo.AddDays(-days);

        int limit = 100;
        int offset = 0;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        while (true)
        {
            var url = $"{_ordersUrl}?" +
                      $"date_from={dateFrom:yyyy-MM-dd HH:mm:ss}" +
                      $"&date_to={dateTo:yyyy-MM-dd HH:mm:ss}" +
                      $"&limit={limit}&offset={offset}&state=all";

            Console.WriteLine(url);

            var response = await _client.GetStringAsync(url);

            var result = JsonSerializer.Deserialize<OrderListResponse>(response, options);

            if (result?.orders == null || result.orders.Count == 0)
                break;

            int newCount = 0;

            foreach (var order in result.orders)
            {
                // 🔥 если уже был — пропускаем
                if (loadedIds.Add(order.id))
                {
                    allOrders.Add(order);
                    newCount++;
                }
            }

            Console.WriteLine($"Loaded batch: {result.orders.Count}, new: {newCount}");

            // ❗ КЛЮЧЕВОЕ УСЛОВИЕ ОСТАНОВКИ
            if (newCount == 0)
            {
                Console.WriteLine("No new orders — stopping");
                break;
            }

            offset += limit;
        }

        return allOrders;
    }
}