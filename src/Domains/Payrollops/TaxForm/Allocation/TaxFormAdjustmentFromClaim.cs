namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Functions of tax form adjustment from claim.</summary>
public static class TaxFormAdjustmentFromClaim
{
    public static Dictionary<string, double> SummarizeTaxFormClaimsForAdjustment(List<TaxFormClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<TaxFormAdjustment> DeriveTaxFormAdjustmentsFromClaim(List<TaxFormClaim> items)
    {
        var outValue = new List<TaxFormAdjustment>();
        foreach (var item in items)
        {
            var derived = TaxFormAdjustment.NewTaxFormAdjustment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeTaxFormAdjustment(TaxFormAdjustment item)
    {
        return FallbackTaxFormMetricPolicy.NormalizeFallbackTaxFormMetricCode(item.Code);
    }
}
