namespace UsStatesComparer.Zillow;

public class PropertyRequest
{
    public string operationName { get; set; } = "ForSaleShopperPlatformFullRenderQuery";
    public Variables variables { get; }
    public string clientVersion { get; set; } = "home-details/6.1.1666.master.1f79a43";
    public string queryId { get; set; } = "cb27a45ca5265412904881447d4afe9d";

    public PropertyRequest(string zpid)
    {
        variables = new Variables(zpid);
    }
}

public class Variables
{
    public string zpid { get; }

    public Variables(string zpid)
    {
        this.zpid = zpid;
    }
}

public class ContactFormRenderParameter
{
    public string zpid { get; }
    public string platform { get; set; } =  "desktop";
    public bool isDoubleScroll { get; set; } = true;

    public ContactFormRenderParameter(string zpid)
    {
        this.zpid = zpid;
    }
}
