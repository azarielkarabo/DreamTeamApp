using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AlphaVantageStockApi
{
    class Program
    {
        private const string ApiKey = "589a6d6cedmsh4c0cf365627c871p1156aajsnbebfb7fbb3e5";
        private const string ApiHost = "alpha-vantage.p.rapidapi.com";
        private static readonly string[] Companies = { "MSFT", "AAPL", "NFLX", "FB", "AMZN" };

        static async Task Main()
        {
            Console.WriteLine("Fetching stock data...\n");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", ApiKey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", ApiHost);

                foreach (var company in Companies)
                {
                    await FetchStockData(client, company);
                    await Task.Delay(12000); // Respect API rate limit
                }
            }
        }

        private static async Task FetchStockData(HttpClient client, string symbol)
        {
            string url = $"https://{ApiHost}/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize=compact&datatype=json";

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var stockData = JsonSerializer.Deserialize<StockResponse>(jsonResponse);

                Console.WriteLine($"Stock Data for {symbol}:");
                if (stockData != null && stockData.TimeSeriesDaily != null)
                {
                    var latestDate = stockData.TimeSeriesDaily.Keys.FirstOrDefault();
                    var latestData = stockData.TimeSeriesDaily[latestDate];
                    if (latestData != null)
                    {
                        Console.WriteLine($"Date: {latestDate}");
                        Console.WriteLine($"Open: {latestData.Open}");
                        Console.WriteLine($"High: {latestData.High}");
                        Console.WriteLine($"Low: {latestData.Low}");
                        Console.WriteLine($"Close: {latestData.Close}");
                        Console.WriteLine($"Volume: {latestData.Volume}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data for {symbol}: {ex.Message}\n");
            }
        }
    }

    public class StockResponse
    {
        [JsonPropertyName("Time Series (Daily)")]
        public Dictionary<string, DailyStockData> TimeSeriesDaily { get; set; }
    }

    public class DailyStockData
    {
        [JsonPropertyName("1. open")]
        public string Open { get; set; }

        [JsonPropertyName("2. high")]
        public string High { get; set; }

        [JsonPropertyName("3. low")]
        public string Low { get; set; }

        [JsonPropertyName("4. close")]
        public string Close { get; set; }

        [JsonPropertyName("5. volume")]
        public string Volume { get; set; }
    }
}