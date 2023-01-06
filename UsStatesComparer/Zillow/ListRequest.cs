// ReSharper disable InconsistentNaming
namespace UsStatesComparer.Zillow;

public class ListRequest
{
    public SearchQueryState searchQueryState { get; set; } = new ();
    public Wants wants { get; set; } = new();
    public int requestId { get; set; } = 15;
}


public class SearchQueryState
{
    public MapBounds mapBounds { get; set; } = new();
    public bool isMapVisible { get; set; } = true;
    public FilterState filterState { get; set; } = new();
    public bool isListVisible { get; set; } = true;
}

public class MapBounds
{
    public double north { get; set; } = 36.972375044868;
    public double east { get; set; } = -75.5885714705933;
    public double south { get; set; } = 36.637007258473915;
    public double west { get; set; } = -77.05249969324954;
}

public class FilterState
{
    public MinMax price { get; set; } = new(0, 600_000);
    public Min beds { get; set; } = new(3);
    public Min baths { get; set; } = new(2);
    public Min sqft { get; set; } = new(1500);
    public Min built { get; set; } = new(1980);
    public BoolValue isCondo { get; set; } = new(false);
    public BoolValue isApartment { get; set; } = new(false);
    public BoolValue isMultiFamily { get; set; } = new(false);
    public BoolValue hasAirConditioning { get; set; } = new(true);
    public MinMax monthlyPayment { get; set; } = new(0, 2965);
    public BoolValue isAllHomes { get; set; } = new(true);
    public StringValue sortSelection { get; set; } = new("globalrelevanceex");
    public BoolValue isLotLand { get; set; } = new(false);
    public BoolValue isTownhouse { get; set; } = new(false);
    public BoolValue isMiddleSchool { get; set; } = new(false);
    public BoolValue isHighSchool { get; set; } = new(false);
    public BoolValue includeUnratedSchools { get; set; } = new(false);
    public BoolValue isManufactured { get; set; } = new(false);
    public BoolValue isPublicSchool { get; set; } = new(false);
    public BoolValue isPrivateSchool { get; set; } = new(false);
    public BoolValue isApartmentOrCondo { get; set; } = new(false);
    public BoolValue isElementarySchool { get; set; } = new(false);
    public BoolValue isCharterSchool { get; set; } = new(false);
}

public class MinMax
{
    public int min { get; set; }
    public int max { get; set; }

    public MinMax(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}

public class Min
{
    public int min { get; set; }

    public Min(int min)
    {
        this.min = min;
    }
}

public class BoolValue
{
    public bool value { get; set; }

    public BoolValue(bool value)
    {
        this.value = value;
    }
}

public class StringValue
{
    public string value { get; set; }

    public StringValue(string value)
    {
        this.value = value;
    }
}

public class Wants
{
    public string[] cat1 { get; set; } = {"listResults", "mapResults"};
    public string[] cat2 { get; set; } = {"total"};
    public string[] regionResults { get; set; } = {"regionResults"};
}
