using UsStatesComparer.Zillow.Client;

namespace UsStatesComparer.Zillow;

public static class ZillowEntryPoint
{
    private static readonly HashSet<string> WeirdSids = new() { "1462694" }; // sometimes server respond something wrong. reprocess helps in most cases.
    public static async Task Run()
    {
        LowZillowClient.SetCookie(); // pass cookie from incognito mode to handle 403 HTTP.
        const string SearchName = "Atlanta";
        ListRequest _; // see this class defaults for query.
        // -----------------------------------------------------------------------------

        var client = new ZillowClient(new LowCachedZillowClient($@"D:\Zillow\{SearchName}", @"D:\Zillow\Properties"));
        var propertyList = await GetPropertyListAsync(new MapBounds(), client);

        //await PreloadPropertiesAsync(propertyList, client);
        var properties = await GetProperties(propertyList, client).ToArrayAsync();

        var filtered = properties
            .Where(x => x.schools?.All(s => s.rating > 6) == true)
            .ToList();

        Console.WriteLine($"We are interested in {filtered.Count} properties of {properties.Length}");

        var grouped = filtered
            .GroupBy(x => x.city)
            .Select(x => (city: x.Key, houses: x.OrderByDescending(a => a.schools!.Sum(s => s.rating)).ToArray()))
            .OrderBy(x => x.city)
            .ToList();

        foreach (var x in grouped)
            Console.WriteLine($"{x.houses.Length}\t{x.city}");

        for (var i = 0; i < grouped.Count;)
        {
            var g = grouped[i];
            Console.WriteLine($"Current city: {g.city} ({g.houses.Length}), what to do (open, next, prev, exit)?");

            var line = Console.ReadLine();
            if (line == null || line[0] == 'e')
                break;

            if (line == "")
            {
                continue;
            }

            if (line[0] == 'n')
            {
                if (++i >= grouped.Count)
                    --i;
                continue;
            }

            if (line[0] == 'p')
            {
                if (i > 0)
                    i--;
                continue;
            }

            if (line[0] == 'o')
            {
                OpenGroup(g.houses);
                if (++i >= grouped.Count)
                    --i;
                continue;

            }

            Console.WriteLine("Unknown command");
        }
    }

    private static void OpenGroup(Property[] houses)
    {
        var propertiesEnumerable = houses.Chunk(5).ToArray();
        for (var i = 0; i < propertiesEnumerable.Length; i++)
        {
            var chunk = propertiesEnumerable[i];

            foreach (var property in chunk)
            {
                var url = property.hdpUrl;
                if (string.IsNullOrEmpty(url) || !url.StartsWith("/"))
                {
                    Console.WriteLine($"Weird hdpUrl for {property.zpid}");
                    continue;
                }

                url = $"https://www.zillow.com{url}";
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }

            if (i + 1 == propertiesEnumerable.Length)
                continue;

            Console.WriteLine($"Press enter to open next {chunk.Length} in this city");
            var l = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(l))
            {
                break;
            }
        }
    }

    private static async Task<string[]> GetPropertyListAsync(MapBounds mapBounds, ZillowClient client)
    {
        var result = new HashSet<string>();
        var asyncEnumerable = GetPropertyList(mapBounds, client);
        var stopwatch = Stopwatch.StartNew();
        await foreach (var list in asyncEnumerable)
        {
            foreach (var r in list)
            {
                if (!string.IsNullOrEmpty(r.zpid))
                {
                    result.Add(r.zpid);
                    if (stopwatch.Elapsed.TotalSeconds > 5)
                    {
                        Console.WriteLine($"Currently found {result.Count} properties");
                        stopwatch.Restart();
                    }
                }
            }
        }

        Console.WriteLine($"Found {result.Count} properties");
        return result.Where(x => !WeirdSids.Contains(x)).ToArray();
    }

    private static async IAsyncEnumerable<IEnumerable<Result>> GetPropertyList(MapBounds mapBounds, ZillowClient client)
    {
        var current = await client.GetListAsync(mapBounds);
        if (current?.cat1?.searchResults?.mapResults == null)
            throw new Exception("GetPropertyListAsync: Error 1");
        if (current.cat1.searchResults.listResults == null)
            throw new Exception("GetPropertyListAsync: Error 2");
        if (current.cat1.searchList == null)
            throw new Exception("GetPropertyListAsync: Error 3");

        var mapResults = current.cat1.searchResults.mapResults!;
        var listResults = current.cat1.searchResults.listResults!;

        yield return mapResults.Concat(listResults);

        var totalResultCount = current.cat1.searchList.totalResultCount!;

        if (totalResultCount <= 300)
            yield break;

        if (mapResults.Length >= totalResultCount)
            yield break;

        if (listResults.Length >= totalResultCount)
            yield break;

        foreach (var q in mapBounds.Split())
        {
            await foreach (var r in GetPropertyList(q, client))
            {
                yield return r;
            }

        }
    }

    private static async IAsyncEnumerable<Property> GetProperties(string[] zpids, ZillowClient client)
    {
        var stopwatch = Stopwatch.StartNew();

        for (var i = 0; i < zpids.Length; i++)
        {
            var zpid = zpids[i];

            var property = await client.GetPropertyAsync(zpid);
            var p = property?.data?.property;

            if (p == null)
            {
                Console.WriteLine($"'{zpid}'");
            }
            else
            {
                yield return p;
            }

            if (stopwatch.Elapsed.TotalSeconds > 5)
            {
                Console.WriteLine($"processed {i + 1}/{zpids.Length} properties");
                stopwatch.Restart();
            }
        }

        Console.WriteLine($"processed {zpids.Length}/{zpids.Length} properties");
    }

    private static async Task PreloadPropertiesAsync(string[] zpids, ZillowClient client)
    {
        var stopwatch = Stopwatch.StartNew();
        var tasks = new Task[zpids.Length];
        var ss = new SemaphoreSlim(64);
        for (var i = 0; i < zpids.Length; i++)
        {
            var num = i;
            await ss.WaitAsync();
            var zpid = zpids[i];
            tasks[i] = client.GetPropertyAsync(zpid).ContinueWith(x =>
            {
                ss.Release();
                if (x.IsFaulted)
                {
                    Console.WriteLine($"Failed {num}");
                    i = Int32.MaxValue;
                }
            });

            if (stopwatch.Elapsed.TotalSeconds > 5)
            {
                Console.WriteLine($"processed {i + 1}/{zpids.Length} properties");
                stopwatch.Restart();
            }
        }

        await Task.WhenAll(tasks);
    }
}
