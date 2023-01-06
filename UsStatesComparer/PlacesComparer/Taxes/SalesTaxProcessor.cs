using CoreLib.Csv;

namespace UsStatesComparer.PlacesComparer;

public static class SalesTaxProcessor
{
    /// <returns>State - Percent</returns>
    public static async Task<Dictionary<State, double>> GetSalesTaxesAsync()
    {
        return ParseInput(await StaticCsvReader.ReadAsync("PlacesComparer/data/sales_tax.csv"))
            .ToDictionary(x => x.State, x => x.MaxTax);
    }

    private static IEnumerable<(State State, double MaxTax)> ParseInput(CsvFile file)
    {
        return file.Select(
            x =>
            {
                var stateName = x[0];
                var index = stateName.IndexOf('(');
                if (index >= 0)
                {
                    stateName = stateName.Substring(0, index).TrimEnd();
                }

                var state = StateMapper.FromString(stateName);
                var stateTaxRate = x["State Tax Rate"].TrimEnd('%');
                var maxLocalTaxRate = x["Max Local Tax Rate"].TrimEnd('%');
                return (state, double.Parse(stateTaxRate) + double.Parse(maxLocalTaxRate));
            });
    }

    private class TaxInputState
    {
        public State State { get; init; }
        public List<TaxInputLine> Taxes { get; } = new List<TaxInputLine>();
    }

    private class TaxInputLine
    {
        public double From { get; init; }
        public double Percent { get; init; }
    }
}
