namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Functions of legacy asset return plan from claim.</summary>
public static class LegacyAssetReturnPlanFromClaim
{
    public static Dictionary<string, double> SummarizeRollingLaptopClaimsForPlan(List<RollingLaptopClaim> items)
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

    public static List<LegacyAssetReturnPlan> DeriveLegacyAssetReturnPlansFromClaim(List<RollingLaptopClaim> items)
    {
        var outValue = new List<LegacyAssetReturnPlan>();
        foreach (var item in items)
        {
            var derived = LegacyAssetReturnPlan.NewLegacyAssetReturnPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacyAssetReturnPlan(LegacyAssetReturnPlan item)
    {
        return RollingLaptopClaimPolicy.RollingLaptopClaimBucket(item.MeasuredValue());
    }
}
