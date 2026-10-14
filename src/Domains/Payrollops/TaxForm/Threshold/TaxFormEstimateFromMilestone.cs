namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Functions of tax form estimate from milestone.</summary>
public static class TaxFormEstimateFromMilestone
{
    public static Dictionary<string, double> SummarizePayRunMilestonesForEstimate(List<PayRunMilestone> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<TaxFormEstimate> DeriveTaxFormEstimatesFromMilestone(List<PayRunMilestone> items)
    {
        var outValue = new List<TaxFormEstimate>();
        foreach (var item in items)
        {
            var derived = TaxFormEstimate.NewTaxFormEstimate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleTaxFormEstimate(TaxFormEstimate item)
    {
        return DefaultPayRunRequestPolicy.DefaultPayRunRequestWeight(7, true);
    }
}
