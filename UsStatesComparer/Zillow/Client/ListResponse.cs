// ReSharper disable InconsistentNaming
namespace UsStatesComparer.Zillow.Client;

public class ListResponse
{
    public Cat1? cat1 { get; set; }
}

public class Cat1
{
    public SearchResults? searchResults { get; set; }
    public SearchList? searchList { get; set; }
}

public class SearchList
{
    public PaginationResponse? pagination { get; set; }
    public int? totalResultCount { get; set; }
}

public class PaginationResponse
{
    public string? nextUrl { get; set; }
}

public class SearchResults
{
    public Result[]? listResults { get; set; }
    public Result[]? mapResults { get; set; }
}

public class Result
{
    public string? zpid { get; set; }
}
