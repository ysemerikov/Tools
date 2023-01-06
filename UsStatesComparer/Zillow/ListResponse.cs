// ReSharper disable InconsistentNaming
namespace UsStatesComparer.Zillow;

public class ListResponse
{
    public Cat1? cat1 { get; set; }
}

public class Cat1
{
 public SearchResults? searchResults { get; set; }
}

public class SearchResults
{
public Result[]? mapResults { get; set; }
}

public class Result
{
    public string? zpid { get; set; }
}
