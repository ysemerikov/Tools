using CoreLib.Csv;

namespace UsStatesComparer.PlacesComparer;

public static class IncomeTaxProcessor
{
    public static async Task<Dictionary<State, (double FederalTax, double StateTax)>> CalculateTaxesAsync(double earlyIncome)
    {
        var federal = new List<TaxInputLine>
        {
            new() {From = 0, Percent = 10},
            new() {From = 20_550, Percent = 12},
            new() {From = 83_550, Percent = 22},
            new() {From = 178_150, Percent = 24},
            new() {From = 340_100, Percent = 32},
            new() {From = 431_900, Percent = 35},
            new() {From = 647_850, Percent = 37},
        };

        var federalTax = GetTax(earlyIncome, federal);
        return ParseInput(await StaticCsvReader.ReadAsync("PlacesComparer/data/income_tax.csv"))
            .ToDictionary(x => x.State, x => (federalTax, GetTax(earlyIncome, x.Taxes)));
    }

    private static double GetTax(double earlyIncome, List<TaxInputLine> taxBaskets)
    {
        var totalTax = 0d;

        var lastFrom = taxBaskets[0].From;
        var lastPercent = taxBaskets[0].Percent;
        foreach (var basket in taxBaskets.Skip(1))
        {
            var currentFrom = basket.From;
            if (currentFrom >= earlyIncome)
            {
                totalTax+= (earlyIncome - lastFrom) * lastPercent / 100;
                return totalTax;
            }

            totalTax += (currentFrom - lastFrom) * lastPercent / 100;
            lastFrom = basket.From;
            lastPercent = basket.Percent;
        }

        totalTax+= (earlyIncome - lastFrom) * lastPercent / 100;
        return totalTax;
    }

    private static IEnumerable<TaxInputState> ParseInput(CsvFile file)
    {
        var current = default(TaxInputState?);
        foreach (var line in file.Skip(2))
        {
            var percentCell = line[4];
            var signCell = line[5];
            var limitCell = line[6];

            TaxInputLine taxInputLine;
            if (percentCell.Equals("none") ||
                percentCell == "5% on interest and dividends only" ||
                percentCell == "7.0% on capital gains income only")
            {
                taxInputLine = new TaxInputLine {From = 0, Percent = 0};
            }
            else if (signCell != ">" || !percentCell.EndsWith('%') || !limitCell.StartsWith('$'))
            {
                throw new Exception(line.ToString());
            }
            else
            {
                var fromStr = limitCell.TrimStart('$').Replace(",", "");
                var percentStr = percentCell.TrimEnd('%');
                taxInputLine = new TaxInputLine
                {
                    From = int.Parse(fromStr),
                    Percent = double.Parse(percentStr),
                };
            }

            var stateNameCell = line[0];
            var index = stateNameCell.IndexOf('(');
            if (index >= 0)
            {
                stateNameCell = stateNameCell.Substring(0, index).TrimEnd();
            }

            if (!string.IsNullOrEmpty(stateNameCell))
            {
                if (current != null)
                {
                    yield return current;
                }

                var state = StateMapper.FromString(stateNameCell);
                current = new TaxInputState {State = state};
            }

            current!.Taxes.Add(taxInputLine);
        }

        if (current != null)
            yield return current;
    }

    private class TaxInputState
    {
        public State State { get; init; }
        public List<TaxInputLine> Taxes { get; } = new();
    }

    private class TaxInputLine
    {
        public double From { get; init; }
        public double Percent { get; init; }
    }
}
