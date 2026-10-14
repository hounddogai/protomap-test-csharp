namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Functions of refresh adjustment from estimate.</summary>
public static class RefreshAdjustmentFromEstimate
{
    public static Dictionary<string, double> SummarizeWeightedLaptopEstimatesForAdjustment(List<WeightedLaptopEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RefreshAdjustment> DeriveRefreshAdjustmentsFromEstimate(List<WeightedLaptopEstimate> items)
    {
        var outValue = new List<RefreshAdjustment>();
        foreach (var item in items)
        {
            var derived = RefreshAdjustment.NewRefreshAdjustment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchRefreshAdjustment(RefreshAdjustment item)
    {
        return PendingLaptopTargetPolicy.PendingLaptopTargetBucket(item.ProjectedLoad());
    }
}
