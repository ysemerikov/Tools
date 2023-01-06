namespace UsStatesComparer.PlacesComparer;

public static class EntryPoint
{
    public static async Task Compare()
    {
        const double earlyIncome = 100_000_000;
        var taxes = await TaxProcessor.CalculateAllTaxesAsync(earlyIncome, 12 * 4_000);

        var federalTax = taxes.First().Value.FederalTax;
        var afterFederalTax = earlyIncome - federalTax;

        var result = taxes
            .Select(x => (x.Key, x.Value, afterFederalTax - x.Value.StateTax - x.Value.SalesTax))
            .OrderByDescending(x => x.Item3);


        Console.WriteLine("State,Income Tax,SalesTax,Rest");
        Console.WriteLine($"Federal,{federalTax:F0},,{afterFederalTax:F0}");
        foreach (var elem in result)
        {
            Console.WriteLine($"{elem.Key:G},{elem.Value.StateTax:F0},{elem.Value.SalesTax:F0},{elem.Item3:F0}");
        }
    }
}
