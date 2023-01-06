using Newtonsoft.Json;

namespace UsStatesComparer.Zillow;

public abstract class CachedRequest<T>
{
    protected readonly string CacheFolder;

    /// <param name="useCache">true - use, false - do not use, null - use if available.</param>
    public async Task<T?> GetAsync(bool? useCache = null)
    {
        string? json = null;
        var fromCache = true;

        if (useCache != false)
        {
            var cachedData = await ReadCachedDataAsync();
            if (string.IsNullOrEmpty(cachedData))
            {
                if (useCache == true)
                {
                    throw new Exception("Nothing in the cache");
                }

                json = null;
            }
            else
            {
                json = cachedData;
            }
        }

        if (json == null)
        {
            json = await ReadFreshDataAsync();
            fromCache = false;
        }

        if (json == null)
            return default;

        var result = ToResult(json);
        if (result != null && !fromCache)
        {
            await WriteToCache(json);
        }

        return result;
    }

    private async ValueTask<string?> ReadCachedDataAsync()
    {
        var cachedFileName = GetCacheFileName();
        if (cachedFileName == null || !File.Exists(cachedFileName))
        {
            return default;
        }

        return await File.ReadAllTextAsync(cachedFileName);
    }


    protected abstract string? GetCacheFileName();
    protected abstract Task<string?> ReadFreshDataAsync();
    protected abstract T? ToResult(string json);
    protected abstract Task WriteToCache(string json);

    protected CachedRequest(string folderName)
    {
        CacheFolder = $@"D:\Zillow\{folderName}\";
    }
}

public class CachedListRequest : CachedRequest<Result[]>
{
    protected override string? GetCacheFileName()
    {
        return Directory.EnumerateFiles(CacheFolder).Max();
    }

    protected override Task<string?> ReadFreshDataAsync()
    {
        var request = new ListRequest();
        return new Client().ListAsync(request);
    }

    protected override Result[]? ToResult(string json)
    {
        var result = JsonConvert.DeserializeObject<ListResponse>(json)?.cat1?.searchResults?.mapResults;
        return result == null || result.Length == 0 ? null : result;
    }

    protected override Task WriteToCache(string json)
    {
        return File.WriteAllTextAsync($"{CacheFolder}{DateTime.Now:MM-dd HH-mm-ss}.json", json);
    }

    public CachedListRequest() : base("List")
    {
    }
}

public class CachedPropertyRequest : CachedRequest<Property>
{
    private readonly string _zpid;

    protected override string GetCacheFileName()
    {
        return $"{CacheFolder}{_zpid}.json";
    }

    protected override Task<string?> ReadFreshDataAsync()
    {
        return new Client().GetPropertyAsync(_zpid);
    }

    protected override Property? ToResult(string json)
    {
        var result = JsonConvert.DeserializeObject<PropertyResponse>(json)?.data?.property;
        if (result == null)
            return null;

        if (result.zpid != _zpid)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"WTF with {_zpid}");
            Console.ForegroundColor = color;
        }

        return result;
    }

    protected override Task WriteToCache(string json)
    {
        return File.WriteAllTextAsync(GetCacheFileName(), json);
    }

    public CachedPropertyRequest(string zpid) : base("Properties")
    {
        _zpid = zpid;
    }
}
