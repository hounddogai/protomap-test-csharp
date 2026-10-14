namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

/// <summary>Functions of weighted accrual bundle from manifest.</summary>
public static class WeightedAccrualBundleFromManifest
{
    public static Dictionary<string, double> SummarizeBatchedReturnPlanManifestsForBundle(List<BatchedReturnPlanManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<WeightedAccrualBundle> DeriveWeightedAccrualBundlesFromManifest(List<BatchedReturnPlanManifest> items)
    {
        var outValue = new List<WeightedAccrualBundle>();
        foreach (var item in items)
        {
            var derived = WeightedAccrualBundle.NewWeightedAccrualBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeWeightedAccrualBundle(WeightedAccrualBundle item)
    {
        return DefaultReturnPlanQuotaPolicy.DefaultReturnPlanQuotaBucket(item.NetExposure());
    }
}
