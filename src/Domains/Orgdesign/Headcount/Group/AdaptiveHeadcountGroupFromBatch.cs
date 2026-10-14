namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Functions of adaptive headcount group from batch.</summary>
public static class AdaptiveHeadcountGroupFromBatch
{
    public static Dictionary<string, double> SummarizeBatchedCostModelBatchesForGroup(List<BatchedCostModelBatch> items)
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

    public static List<AdaptiveHeadcountGroup> DeriveAdaptiveHeadcountGroupsFromBatch(List<BatchedCostModelBatch> items)
    {
        var outValue = new List<AdaptiveHeadcountGroup>();
        foreach (var item in items)
        {
            var derived = AdaptiveHeadcountGroup.NewAdaptiveHeadcountGroup(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeAdaptiveHeadcountGroup(AdaptiveHeadcountGroup item)
    {
        return LegacyCostModelGroupPolicy.LegacyCostModelGroupWeight(4, true);
    }
}
