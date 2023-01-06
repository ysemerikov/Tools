using Newtonsoft.Json;

namespace UsStatesComparer.Zillow;

public class Client
{
    private static readonly HttpClient _client = new ();

    public Task<string?> ListAsync(ListRequest request)
    {
        var searchQueryState = JsonConvert.SerializeObject(request.searchQueryState);
        var wants = JsonConvert.SerializeObject(request.wants);
        var requestId = request.requestId.ToString();
        var url = $"https://www.zillow.com/search/GetSearchPageState.htm?searchQueryState={searchQueryState}&wants={wants}&requestId={requestId}";

        return Get(url);
    }

    public async Task<string?> GetPropertyAsync(string zpid)
    {
        var url = $"https://www.zillow.com/graphql/?zpid={zpid}&contactFormRenderParameter=&queryId=cb27a45ca5265412904881447d4afe9d&operationName=ForSaleShopperPlatformFullRenderQuery";
        var body = JsonConvert.SerializeObject(new PropertyRequest(zpid));

        using var content = new StringContent(body, null, "application/json");
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content,
        };

        using var response = await _client.SendAsync(message, HttpCompletionOption.ResponseContentRead);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string?> Get(string url)
    {
        using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    static Client()
    {
        var headers = _client.DefaultRequestHeaders;
        headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        headers.Add("cookie", "zguid=24|$f58b9de4-168a-4911-b53a-9d4e23c68aab; zgsession=1|c254c853-885f-48cc-a557-f184eb349295; _ga=GA1.2.1518546821.1666917363; zjs_user_id=null; zg_anonymous_id=\"afc92d75-ac3d-4538-b9c2-26f53f506499\"; zjs_anonymous_id=\"f58b9de4-168a-4911-b53a-9d4e23c68aab\"; pxcts=806bf4e8-5658-11ed-88fd-687a445a7943; _pxvid=806be8a6-5658-11ed-88fd-687a445a7943; _gcl_au=1.1.1301510932.1666917365; DoubleClickSession=true; __pdst=2c84196be4424363a99e148b2dde5794; _pin_unauth=dWlkPU0yUTJZVFF3WVRjdE9EaGhZaTAwTkdRekxUazBOamd0WkdGa04yVTNaR1k1WlRVNQ; _cs_c=0; optimizelyEndUserId=oeu1668474140545r0.8226463577554652; zgcus_aeut=AEUUT_28150bee-6481-11ed-a189-f2c29f2c2a39; zgcus_aeuut=AEUUT_28150bee-6481-11ed-a189-f2c29f2c2a39; utag_main=v_id:01841c04761f0015c609d809c21e0506f005806700bd0$_sn:5$_se:2$_ss:0$_st:1670354157208$dc_visit:5$ses_id:1670352327295;exp-session$_pn:2;exp-session$dcsyncran:1;exp-session$tdsyncran:1;exp-session$dc_event:1;exp-session; FSsampler=1454752727; g_state={\"i_p\":1672632482033,\"i_l\":3}; _gid=GA1.2.1009375245.1672716861; _hp2_id.1215457233={\"userId\":\"1814386082850923\",\"pageviewId\":\"6890921627190161\",\"sessionId\":\"8534238784456818\",\"identity\":null,\"trackerVersion\":\"4.0\"}; _cs_id=958d1c26-0606-a122-fb16-03493728fa7a.1668473960.13.1672716862.1672716862.1.1702637960326; _clck=ca0wxv|1|f7y|0; JSESSIONID=C6B811184248D3B1F800B1BC1860BCC0; QSI_SI_3JeA27i8MrWZTYR_intercept=true; _uetsid=83e24fb08b1711eda0ea2d91968ea8db; _uetvid=13de3780eb8f11ec8a0addda4cc25762; _px3=d763f48046febb5b26d42cfd3ac6b7997fd2118fefbf4b9f3688ea4cc7f769a2:Dq3PPX8xjoMayr8DdNmKvB39gCRySvtgbYp2qvgx1CKzfbYAMwLmKUTmuBega32UTqjRqfLsxnYcqyfUJGnwKg==:1000:vfK5L4ErcjHzmUSi/JjP3jYviQXcsw5zdoN7E6Guiwt5FH5bOO23uvdpMqVz8zWTY0pQsjIe+x66EYxofENYiqSIEg1vJ7Y57SagZwwQrIbikItfdM0HX95zv5Wd5x0YVyu15dsDs46SqPDiAT0oHWZFfCO2hGN2BxDosZmwPQuf+GlhOqpmIKA00aOjrEYWC0UCrMS78f2lcZp+Vxqn/A==; _gat=1; _clsk=23bw5m|1672719598924|219|0|f.clarity.ms/collect; AWSALB=/wsyxn2F86kwT7mnkRvwZgXt4p5Ej9d0Fkz6ZSr+LrX4C5omGe7l7DC10M3F5SIt/gnoO37zgvXtLdJZ80h/REc488DMT3++SGHCI9sANsvoVHkJA3qOFJJttF4H; AWSALBCORS=/wsyxn2F86kwT7mnkRvwZgXt4p5Ej9d0Fkz6ZSr+LrX4C5omGe7l7DC10M3F5SIt/gnoO37zgvXtLdJZ80h/REc488DMT3++SGHCI9sANsvoVHkJA3qOFJJttF4H; search=6|1675311598313|rect=37.28387556941155%2C-75.5885714705933%2C36.32285879207021%2C-77.05249969324954&disp=map&mdm=auto&p=1&z=1&baths=2.0-&beds=3-&type=house&price=0-600000&mp=0-2965&fs=1&fr=0&mmm=0&rs=0&ah=0&singlestory=0&size=1500-&built=1980-&housing-connector=0&abo=0&garage=0&pool=0&ac=1&waterfront=0&finished=0&unfinished=0&cityview=0&mountainview=0&parkview=0&waterview=0&hoadata=1&zillow-owned=0&3dhome=0&featuredMultiFamilyBuilding=0&commuteMode=driving&commuteTimeOfDay=now		41515						");
    }
}
