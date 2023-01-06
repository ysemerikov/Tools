namespace UsStatesComparer.PlacesComparer;

public static class TaxProcessor
{
    /// <param name="earlyIncome"></param>
    /// <param name="taxableEarlySpending">0 or less means everything</param>
    public static async Task<Dictionary<State, (double FederalTax, double StateTax, double SalesTax)>> CalculateAllTaxesAsync(double earlyIncome, double taxableEarlySpending = 0)
    {
        var incomeTaxDict = await IncomeTaxProcessor.CalculateTaxesAsync(earlyIncome);
        var salesTaxDict = await SalesTaxProcessor.GetSalesTaxesAsync();

        return StateMapper.AllStates
            .ToDictionary(x=>x.Value,
                x =>
                {
                    var state = x.Value;
                    var incomeTax = incomeTaxDict[state];
                    var salesTaxable = earlyIncome - incomeTax.FederalTax - incomeTax.StateTax;

                    if (taxableEarlySpending > salesTaxable)
                    {
                        throw new Exception($"You cannot spend more that have in the {x.Name} state.");
                    }

                    if (taxableEarlySpending > 0)
                    {
                        salesTaxable = taxableEarlySpending;
                    }

                    return (incomeTax.FederalTax, incomeTax.StateTax, salesTaxable * salesTaxDict[state] / 100);
                });
    }
}
