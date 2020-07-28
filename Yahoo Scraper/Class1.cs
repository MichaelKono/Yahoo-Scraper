using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace StockScraping
{
    // All of these classes hold returned data
    public class YahooResponse
    {
        public OptionChainResult OptionChain { get; set; }
    }
    public class OptionChainResult
    {
        public List<OptionChainCollection> Result { get; set; }
    }

    public class OptionChainCollection
    {
        public string UnderlyingSymbol { get; set; }
        public UnderlyingQuote Quote { get; set; }

        public List<long> ExpirationDates { get; set; }
        public List<decimal> Strikes { get; set; }
        public List<OptionChain> Options { get; set; }
    }

    public class UnderlyingQuote
    {
        public long DividendDate { get; set; }
        public long EarningsTimestamp { get; set; }
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public long regularMarketTime { get; set; }
        public decimal regularMarketPreviousClose { get; set; }
        public decimal regularMarketPrice { get; set; }
        public decimal regularMarketDayHigh { get; set; }
        public decimal regularMarketDayLow { get; set; }
        public decimal regularMarketChange { get; set; }
        public decimal regularMarketChangePercent { get; set; }
        public int regularMarketVolume { get; set; }
        public string MarketState { get; set; }
    }

    public class OptionChain
    {
        public long ExpirationDate { get; set; }
        public List<Option> Calls { get; set; }
        public List<Option> Puts { get; set; }
    }

    public class Option
    {
        public string ContractSymbol { get; set; }
        public decimal Strike { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal LastPrice { get; set; }
        public decimal Change { get; set; }
        public decimal PercentChange { get; set; }
        public long LastTradeDate { get; set; }
        public int Volume { get; set; }
        public int OpenInterest { get; set; }
        public decimal ImpliedVolatility { get; set; }
        public string ContractSize { get; set; }
        public long Expiration { get; set; }
    }

    public class YahooScrape
    {

        const string _Y_API = "https://query2.finance.yahoo.com/v7/finance/options/"; // The API we access to scrape, we add the callsign to the end of this string when scraping

        // Attempts to find Cached scrapes, otherwise initilizes API request
        private static bool TryGetFromCache(string filename, string fileLocation, TimeSpan expiration, out string data)
        {
            string path = Path.Combine(fileLocation, filename);

            if (File.GetLastWriteTimeUtc(path).Add(expiration) > DateTime.UtcNow)
            {
                data = File.ReadAllText(path);
                return true;
            }

            data = null;
            return false;
        }

        // This saves the scrape localy if TryGetFromCache Fails
        private static void SaveToCache(string filename, string fileLocation, string data)
        {
            string path = Path.Combine(fileLocation, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, data);
        }

        // This actully makes the Http request and scrapes the data
        private static string HttpGet(string symbol, string fileLocation, TimeSpan cacheExpiration, long expirationDate = 0)
        {
            string url = $"{_Y_API}{symbol}";
            string fileName = $"{symbol}\\{symbol}.json";

            if (expirationDate > 0)
            {
                url += $"?date={expirationDate}";
                fileName = $"{symbol}\\{symbol}-{expirationDate}.json";
            }

            string data;

            if (TryGetFromCache(fileName, fileLocation, cacheExpiration, out data))
                return data;

            var client = new HttpClient();
            data = client.GetStringAsync(url).Result;

            SaveToCache(fileName, fileLocation, data);
            // Data = the scraped information in raw Json, we will filter this out later in order to display in a readable format
            return data;
        }

        public static OptionChainCollection GetOptionChainCollection(string symbol, string fileLocation, TimeSpan cacheExpiration)
        {
            // Here you can change the deserialization of the returned data, for now we remove case sensitivity
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            //options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            // Here is where most of our methods would be called once GetOptionChainCollection is initilized. This is also where all our classes created at the top also get populated with the folowing values
            string jsonString = HttpGet(symbol, fileLocation, cacheExpiration);

            var response = JsonSerializer.Deserialize<YahooResponse>(jsonString, options);

            response.OptionChain.Result[0].Options.Clear();

            foreach (long expirationDate in response.OptionChain.Result[0].ExpirationDates)
            {
                jsonString = HttpGet(symbol, fileLocation, cacheExpiration, expirationDate);
                var chain = JsonSerializer.Deserialize<YahooResponse>(jsonString, options);
                response.OptionChain.Result[0].Options.AddRange(chain.OptionChain.Result[0].Options);
            }

            return response.OptionChain.Result[0];
        }

    }
}