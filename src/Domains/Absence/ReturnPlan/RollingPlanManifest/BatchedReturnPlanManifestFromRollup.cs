namespace Acme.Workforce.Absence.ReturnPlan.RollingPlanManifest;

using Acme.Workforce.Absence.ReturnPlan.Entry;

/// <summary>Functions of batched return plan manifest from rollup.</summary>
public static class BatchedReturnPlanManifestFromRollup
{
    public static Dictionary<string, double> SummarizeSeasonalReturnPlanRollupsForManifest(List<SeasonalReturnPlanRollup> items)
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

    public static List<BatchedReturnPlanManifest> DeriveBatchedReturnPlanManifestsFromRollup(List<SeasonalReturnPlanRollup> items)
    {
        var outValue = new List<BatchedReturnPlanManifest>();
        foreach (var item in items)
        {
            var derived = BatchedReturnPlanManifest.NewBatchedReturnPlanManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectBatchedReturnPlanManifest(BatchedReturnPlanManifest item)
    {
        return AdaptiveReturnPlanEntryPolicy.NormalizeAdaptiveReturnPlanEntryCode(item.Code);
    }
}
