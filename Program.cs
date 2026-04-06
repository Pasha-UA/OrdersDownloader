using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using OrdersDownloader.Models;
using OrdersDownloader.Services;

class Program
{
    static async Task Main()
    {
        // ✅ Конфигурация (JSON + ENV + UserSecrets)
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

#if DEBUG
        builder.AddUserSecrets<Program>();
#endif

        var config = builder.Build();

        // ✅ читаем настройки
        var baseUrl = config["PromApi:BaseUrl"];
        var ordersUrl = config["PromApi:OrdersListApiUrl"];
        var token = config["PromApi:Token"]; // 👈 ВАЖНО

        if (string.IsNullOrWhiteSpace(token))
            throw new Exception("❌ PromApi:Token not configured");

        Console.WriteLine("✅ Token loaded");

        // ✅ HttpClient с токеном
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // ✅ создаём API клиент (НОВАЯ сигнатура)
        var client = new PromApiClient(
            httpClient,
            baseUrl,
            ordersUrl
        );

        // 🔥 сколько дней грузим
        int days = 10;

        var orders = await client.GetOrdersForPeriodAsync(days);

        Console.WriteLine($"Loaded: {orders.Count}");

        // 💾 CML
        var exporter = new OrderExporter();
        exporter.ExportToCml(orders, "orders_export.cml");
    }
}