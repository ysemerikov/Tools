namespace UsStatesComparer.Zillow.Client;

public interface ILowZillowClient
{
    Task<string?> GetListAsync(MapBounds mapBounds, int page = 1);
    Task<string?> GetPropertyAsync(string zpid);
}
