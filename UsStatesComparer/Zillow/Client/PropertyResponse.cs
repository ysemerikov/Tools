// ReSharper disable InconsistentNaming
#pragma warning disable CS8618
namespace UsStatesComparer.Zillow.Client;

public class PropertyResponse
{
public Data? data { get; set; }
}

public class Data
{
    public Property? property { get; set; }
}

public class Property
{
    public string zpid { get; set; }
    public string city { get; set; }
    public string state { get; set; }
    public string homeStatus { get; set; }
    public object address { get; set; }
    public string bedrooms { get; set; }
    public string bathrooms { get; set; }
    public int price { get; set; }
    public int yearBuilt { get; set; }
    public string streetAddress { get; set; }
    public string zipcode { get; set; }
    public string regionString { get; set; }
    public string hdpUrl { get; set; }
    public string homeType { get; set; }
    public string currency { get; set; }
    public string livingArea { get; set; }
    public string livingAreaValue { get; set; }
    public School[]? schools { get; set; }
}

public class School
{
    public string distance { get; set; }
    public string name { get; set; }
    public int? rating { get; set; }
    public string level { get; set; }
    public string studentsPerTeacher { get; set; }
    public string assigned { get; set; }
    public string grades { get; set; }
    public string link { get; set; }
    public string type { get; set; }
    public string size { get; set; }
    public string totalCount { get; set; }
    public string isAssigned { get; set; }
}
