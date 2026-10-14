namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Functions of retro pay variance from plan.</summary>
public static class RetroPayVarianceFromPlan
{
    public static Dictionary<string, double> SummarizeRegionalPayRunPlansForVariance(List<RegionalPayRunPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<RetroPayVariance> DeriveRetroPayVariancesFromPlan(List<RegionalPayRunPlan> items)
    {
        var outValue = new List<RetroPayVariance>();
        foreach (var item in items)
        {
            var derived = RetroPayVariance.NewRetroPayVariance(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileRetroPayVariance(RetroPayVariance item)
    {
        return PayRunBudgetPolicy.PayRunBudgetBucket(item.MeasuredValue());
    }
}
