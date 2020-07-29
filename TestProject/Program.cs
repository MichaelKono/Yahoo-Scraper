using System;
using StockScraping;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Runtime.InteropServices;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string symbol = "AAPL"; // Symbol is your ticker, "AAPL" for Apple inc.
            string filelocation = "c://"; // Where you want the cached returns to be kept and searched
            TimeSpan cacheExpiration = TimeSpan.FromMinutes(60); // How long until Yahoo will be scraped again

            OptionChainCollection response = YahooScrape.GetOptionChainCollection(symbol, filelocation, cacheExpiration);

            var results = new List<(Option option, double apy)>(); // Declare new list to keep filtered option data in.

            foreach (OptionChain optionChain in response.Options)
            {
                foreach (Option put in optionChain.Puts) // Reminder you can access both Puts and Calls separately.
                {
                    double daysToExpiration = (DateTimeOffset.FromUnixTimeSeconds(put.Expiration) - DateTimeOffset.UtcNow).TotalDays; // How long until option expires
                    double apy = Math.Pow(1.0 + (double)put.LastPrice / (double)response.Quote.regularMarketPrice, (365 / daysToExpiration)) - 1.0; // Annual percentage yield
                    
                    results.Add((put, apy));
                }

                var options = new JsonSerializerOptions // Make output of serialization easier to read
                { WriteIndented = true};

                foreach (var item in results.OrderByDescending(x => x.apy).Take(10)) // Prints 10 inputs of data, remove .Take(10) to print all data
                {
                    string finished = JsonSerializer.Serialize(item.option, options);
                    Console.WriteLine(finished);
                }
            }
        }
    }
}
