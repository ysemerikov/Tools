// ReSharper disable InconsistentNaming
namespace UsStatesComparer.Zillow.Client;

public class ListRequest
{
    public SearchQueryState searchQueryState { get; }
    public Wants wants { get; set; } = new();
    public int requestId { get; set; } = 7;

    public ListRequest(MapBounds mapBounds)
    {
        searchQueryState = new SearchQueryState(mapBounds);
    }
}


public class SearchQueryState
{
    public Pagination? pagination { get; set; }
    public MapBounds mapBounds { get; }
    public bool isMapVisible { get; set; } = true;
    public FilterState filterState { get; set; } = new();
    public bool isListVisible { get; set; } = true;

    public SearchQueryState(MapBounds mapBounds)
    {
        this.mapBounds = mapBounds;
    }
}

public class Pagination
{
    public int currentPage { get; }

    public Pagination(int currentPage)
    {
        this.currentPage = currentPage;
    }
}

public class MapBounds
{
    public double north { get; set; } = 34.2826120387071;
    public double east { get; set; } = -83.68447922261478;
    public double south { get; set; } = 33.3286663910362;
    public double west { get; set; } = -85.14840744527103;

    public MapBounds[] Split()
    {
        if (north * south < 0 || east * west < 0)
            throw new NotSupportedException("MapBounds.Split");

        var midVertical = (north + south) / 2;
        var midHorizontal = (east + west) / 2;
        return new[]
        {
            new MapBounds { north = this.north, east = midHorizontal, south = midVertical, west = this.west },
            new MapBounds { north = this.north, east = this.east, south = midVertical, west = midHorizontal },
            new MapBounds { north = midVertical, east = midHorizontal, south = this.south, west = this.west },
            new MapBounds { north = midVertical, east = this.east, south = this.south, west = midHorizontal },
        };
    }

    public override string ToString()
    {
        return $"{north}!{east}!{south}!{west}";
    }
}

public class FilterState
{
    public MinMax price { get; set; } = new(0, 600_000);
    public MinMax monthlyPayment { get; set; } = new(0, 3049);
    public Min beds { get; set; } = new(3);
    public Min baths { get; set; } = new(2);
    public Min sqft { get; set; } = new(1500);
    // lotSize
    public Min built { get; set; } = new(1980);
    public StringValue sortSelection { get; set; } = new("globalrelevanceex");
    public BoolValue isAllHomes { get; set; } = new(true);
    // ageRestricted55Plus
    public BoolValue isCondo { get; set; } = new(false);
    public BoolValue isMultiFamily { get; set; } = new(false);
    public BoolValue isManufactured { get; set; } = new(false);
    public BoolValue isLotLand { get; set; } = new(false);
    public BoolValue isTownhouse { get; set; } = new(false);
    public BoolValue isApartment { get; set; } = new(false);
    public BoolValue isApartmentOrCondo { get; set; } = new(false);
    //public BoolValue hasAirConditioning { get; set; } = new(true);
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
