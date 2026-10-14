namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Estimate;

/// <summary>Functions of asset return summary from record.</summary>
public static class AssetReturnSummaryFromRecord
{
    public static Dictionary<string, double> SummarizeScheduledAssetReturnRecordsForSummary(List<ScheduledAssetReturnRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AssetReturnSummary> DeriveAssetReturnSummariesFromRecord(List<ScheduledAssetReturnRecord> items)
    {
        var outValue = new List<AssetReturnSummary>();
        foreach (var item in items)
        {
            var derived = AssetReturnSummary.NewAssetReturnSummary(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateAssetReturnSummary(AssetReturnSummary item)
    {
        return AssetReturnEstimatePolicy.AssetReturnEstimateWeight(4, true);
    }
}
