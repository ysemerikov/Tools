namespace UsStatesComparer.Zillow.Client;

public class LowCachedZillowClient : ILowZillowClient
{
    private readonly string listFolder;
    private readonly string propertiesFolder;
    private readonly LowZillowClient client = new LowZillowClient();

    public async Task<string?> GetListAsync(MapBounds mapBounds, int page = 1)
    {
        var cachedFileName = Path.Combine(listFolder, $"{mapBounds}.json");

        if (File.Exists(cachedFileName))
            return await File.ReadAllTextAsync(cachedFileName);

        var response = await client.GetListAsync(mapBounds, page);
        if (!string.IsNullOrEmpty(response))
        {
            await File.WriteAllTextAsync(cachedFileName, response);
        }

        return response;
    }

    public async Task<string?> GetPropertyAsync(string zpid)
    {
        var cachedFileName = Path.Combine(propertiesFolder, $"{zpid}.json");
        if (File.Exists(cachedFileName))
            return await File.ReadAllTextAsync(cachedFileName);

        var response = await client.GetPropertyAsync(zpid);

        if (!string.IsNullOrEmpty(response))
        {
            await File.WriteAllTextAsync(cachedFileName, response);
        }

        return response;
    }

    public LowCachedZillowClient(string listFolder, string propertiesFolder)
    {
        this.listFolder = listFolder;
        this.propertiesFolder = propertiesFolder;

        if (!Directory.Exists(listFolder))
            Directory.CreateDirectory(listFolder);
        if (!Directory.Exists(propertiesFolder))
            Directory.CreateDirectory(propertiesFolder);
    }
}
