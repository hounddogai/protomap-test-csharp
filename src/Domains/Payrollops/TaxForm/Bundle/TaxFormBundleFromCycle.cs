namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Functions of tax form bundle from cycle.</summary>
public static class TaxFormBundleFromCycle
{
    public static Dictionary<string, double> SummarizeGarnishmentCyclesForBundle(List<GarnishmentCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<TaxFormBundle> DeriveTaxFormBundlesFromCycle(List<GarnishmentCycle> items)
    {
        var outValue = new List<TaxFormBundle>();
        foreach (var item in items)
        {
            var derived = TaxFormBundle.NewTaxFormBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveTaxFormBundle(TaxFormBundle item)
    {
        return GarnishmentMetricPolicy.GarnishmentMetricWeight(9, true);
    }
}
