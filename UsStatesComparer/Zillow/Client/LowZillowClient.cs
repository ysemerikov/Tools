using Newtonsoft.Json;

namespace UsStatesComparer.Zillow.Client;

public class LowZillowClient : ILowZillowClient
{
    private static readonly HttpClient Client = new ();
    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    public Task<string?> GetListAsync(MapBounds mapBounds, int page = 1)
    {
        var request = new ListRequest(mapBounds);
        if (page != 1)
            request.searchQueryState.pagination = new Pagination(page);

        var searchQueryState = JsonConvert.SerializeObject(request.searchQueryState, JsonSerializerSettings);
        var wants = JsonConvert.SerializeObject(request.wants);
        var requestId = request.requestId.ToString();
        var url = $"https://www.zillow.com/search/GetSearchPageState.htm?searchQueryState={searchQueryState}&wants={wants}&requestId={requestId}";

        return Get(url);
    }

    public async Task<string?> GetPropertyAsync(string zpid)
    {
        var url = $"https://www.zillow.com/graphql/?zpid={zpid}&contactFormRenderParameter=&queryId=cb27a45ca5265412904881447d4afe9d&operationName=ForSaleShopperPlatformFullRenderQuery";
        var body = JsonConvert.SerializeObject(new PropertyRequest(zpid), JsonSerializerSettings);

        using var content = new StringContent(body, null, "application/json");
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content,
        };

        using var response = await Client.SendAsync(message, HttpCompletionOption.ResponseContentRead);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string?> Get(string url)
    {
        using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public static void SetCookie(string cookie = "zguid=24|%24afbd1020-9d49-42aa-9438-8f2618adf1e6; zgsession=1|babe21a9-cd6a-4fcc-a308-442c463b2e81; _ga=GA1.2.1314793813.1690428049; _gid=GA1.2.130033331.1690428049; zjs_anonymous_id=%22afbd1020-9d49-42aa-9438-8f2618adf1e6%22; zjs_user_id=null; zg_anonymous_id=%2260ce3585-9225-447f-ae6f-e3c4bcfd85a2%22; _gcl_au=1.1.1989562063.1690428049; DoubleClickSession=true; _pxff_cc=U2FtZVNpdGU9TGF4Ow==; pxcts=9597f63c-2c2c-11ee-83e2-6644734d506b; _pxvid=9597e2b9-2c2c-11ee-83e2-b8060da9e98c; _pxff_cfp=1; _pxff_bsco=1; _gat=1; _hp2_id.1215457233=%7B%22userId%22%3A%223121690014131829%22%2C%22pageviewId%22%3A%228120217706798713%22%2C%22sessionId%22%3A%222604565061280900%22%2C%22identity%22%3Anull%2C%22trackerVersion%22%3A%224.0%22%7D; _px3=c6b19dffddc16d902928294528447db6e93cdfccbc60ff586624d5ce6d59ab49:8XHU4QoZr7OUyNb/P3s4IDX2NbLFxM26H1XogQRlu6CVLqcHuNwwdv8ij155dRfbfH54EvAv1g04z7L0cZg3SA==:1000:gl0YuIfBDiy6BdgA3/DTN59dydoCbOOZC8hNMrQbc5QISZ5T9H57AOWt3N5+eRZ2jA8Y9LgmPY1by030VzpltbHMKcWF4OQyL+qCv90EY3fEskl/Wlx1cv+9OYZ8tKiEx0rVq3D1doGkB+Nv/S4zVZwb9F+BPjz2O0FPdS7WwfSMuwNhXtlML9/10yUL19ekPxBPbM65xMdZJUqHrj9e3w==; _hp2_ses_props.1215457233=%7B%22ts%22%3A1690428049908%2C%22d%22%3A%22www.zillow.com%22%2C%22h%22%3A%22%2F%22%7D; AWSALB=26JSWiMk5k9haA9BWFbZFHRNhJWDEwvqLQ16MQeO9VrPy1U8WGFo2QvDnkKecO87/PbwZviQbKzwrx9mZDHCqbEsUqyVE6fA8NtB6snBmg7jfSjOYj/vfPEE6YYV; AWSALBCORS=26JSWiMk5k9haA9BWFbZFHRNhJWDEwvqLQ16MQeO9VrPy1U8WGFo2QvDnkKecO87/PbwZviQbKzwrx9mZDHCqbEsUqyVE6fA8NtB6snBmg7jfSjOYj/vfPEE6YYV; JSESSIONID=13FE4E0FB6A95D3C188756C357BB5D14; search=6|1693020052523%7Crect%3D34.61%252C-83.5%252C32.61%252C-85.5%26fs%3D1%09%09%09%09%09%09%09%09")
    {
        Client.DefaultRequestHeaders.Add("cookie", cookie);
    }

    static LowZillowClient()
    {
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
    }
}
