namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Functions of tax form request from variance.</summary>
public static class TaxFormRequestFromVariance
{
    public static Dictionary<string, double> SummarizeWeightedGarnishmentVariancesForRequest(List<WeightedGarnishmentVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<TaxFormRequest> DeriveTaxFormRequestsFromVariance(List<WeightedGarnishmentVariance> items)
    {
        var outValue = new List<TaxFormRequest>();
        foreach (var item in items)
        {
            var derived = TaxFormRequest.NewTaxFormRequest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateTaxFormRequest(TaxFormRequest item)
    {
        return LegacyGarnishmentSlotPolicy.LegacyGarnishmentSlotWeight(3, true);
    }
}
