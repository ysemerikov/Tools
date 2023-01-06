using CoreLib;

namespace UsStatesComparer.Zillow;

public static class EntryPoint
{
    private const string CacheRootFolder = @"D:\Zillow\";

    public static async Task Do()
    {
        var list = await new CachedListRequest().GetAsync() ?? throw new Exception("no list");
        var properties = await LoadAllAsync(list);

        var filtered = properties
            .Where(x => x.schools.All(s => s.rating > 6))
            .OrderByDescending(x => x.schools.Sum(s => s.rating));

        var chunkSize = 5;
        foreach (var chunk in filtered.Chunk(chunkSize))
        {
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
                Process.Start(new ProcessStartInfo(url) {UseShellExecute = true});
            }

            Console.WriteLine($"Open next {chunkSize}");
            var line = Console.ReadLine();
            if (!string.IsNullOrEmpty(line))
            {
                Console.WriteLine("quit");
                break;
            }
        }
    }

    private static async Task<List<Property>> LoadAllAsync(Result[] list)
    {
        var result = new List<Property>(list.Length);
        var stopwatch = Stopwatch.StartNew();

        foreach (var zpid in list.Select(x => x.zpid).WhereNotNull())
        {
            var property = await new CachedPropertyRequest(zpid).GetAsync() ??
                           throw new Exception($"Got empty result for {zpid}");

            result.Add(property);

            if (stopwatch.Elapsed.TotalSeconds > 5)
            {
                Console.WriteLine($"processed {result.Count} properties");
                stopwatch.Restart();
            }
        }

        Console.WriteLine($"Total {result.Count} properties.");

        return result;
    }
}
