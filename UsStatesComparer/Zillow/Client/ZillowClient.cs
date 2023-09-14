using Newtonsoft.Json;

namespace UsStatesComparer.Zillow.Client;

public class ZillowClient
{
    private readonly ILowZillowClient client;

    public async Task<ListResponse?> GetListAsync(MapBounds mapBounds, int page = 1)
    {
        var response = await client.GetListAsync(mapBounds, page);
        if (string.IsNullOrEmpty(response))
            return null;

        return JsonConvert.DeserializeObject<ListResponse>(response);
    }

    public async Task<PropertyResponse?> GetPropertyAsync(string zpid)
    {
        var response = await client.GetPropertyAsync(zpid);
        if (string.IsNullOrEmpty(response))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<PropertyResponse>(response);
        }
        catch
        {
            return null;
        }
    }

    public ZillowClient(ILowZillowClient client)
    {
        this.client = client;
    }
}
