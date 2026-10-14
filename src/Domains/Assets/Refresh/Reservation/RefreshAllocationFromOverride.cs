namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Assets.AssetReturn.OverrideItem;

/// <summary>Functions of refresh allocation from override.</summary>
public static class RefreshAllocationFromOverride
{
    public static Dictionary<string, double> SummarizeAssetReturnOverridesForAllocation(List<AssetReturnOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RefreshAllocation> DeriveRefreshAllocationsFromOverride(List<AssetReturnOverride> items)
    {
        var outValue = new List<RefreshAllocation>();
        foreach (var item in items)
        {
            var derived = RefreshAllocation.NewRefreshAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRefreshAllocation(RefreshAllocation item)
    {
        return CappedAssetReturnPlanPolicy.NormalizeCappedAssetReturnPlanCode(item.Code);
    }
}
