# Yahoo-Scraper

Simple Yahoo optionchain scraper.
Scrapes Yahoo's stock API for option data, data can be cached for specified amount of time in order to keep your IP from being blocked from Yahoo's API.

Utilization - 

Create an instance of the OptionChainCollection class, here is an example.
```c#
string symbol; // Symbol is your ticker, "AAPL" for Apple inc.
string filelocation; // Where you want the cached returns to be kept and searched
timespan cacheExpiration = TimeSpan.FromMinutes(60); // How long until Yahoo will be scraped again

OptionChainCollection response = GetOptionChainCollection(symbol, fileLocation, cacheExpiration);
```

To use the data scraped from Yahoo, we will iterate through the data and add what is wanted to another list as shown here.
```c#
var results = new List<(Option option)>(); // Declare new list to keep filtered option data in.

foreach (OptionChain optionChain in response.Options)
            {
                foreach (Option put in optionChain.Puts) // Reminder you can access both Puts and Calls separately.
                {
                    if (put.Volume < minVolume) // Here is an example on filtering out the specified option to display.
                        continue;
                        
                    results.Add((put));
                }
```
