namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Functions of rolling asset return baseline from audit.</summary>
public static class RollingAssetReturnBaselineFromAudit
{
    public static Dictionary<string, double> SummarizeFallbackLicenseSeatAuditsForBaseline(List<FallbackLicenseSeatAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<RollingAssetReturnBaseline> DeriveRollingAssetReturnBaselinesFromAudit(List<FallbackLicenseSeatAudit> items)
    {
        var outValue = new List<RollingAssetReturnBaseline>();
        foreach (var item in items)
        {
            var derived = RollingAssetReturnBaseline.NewRollingAssetReturnBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeRollingAssetReturnBaseline(RollingAssetReturnBaseline item)
    {
        return SeasonalLicenseSeatAssignmentPolicy.SeasonalLicenseSeatAssignmentBucket(item.WeightedValue());
    }
}
