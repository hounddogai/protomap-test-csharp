namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Functions of capped asset return cycle from claim.</summary>
public static class CappedAssetReturnCycleFromClaim
{
    public static Dictionary<string, double> SummarizeWarrantyClaimsForCycle(List<WarrantyClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<CappedAssetReturnCycle> DeriveCappedAssetReturnCyclesFromClaim(List<WarrantyClaim> items)
    {
        var outValue = new List<CappedAssetReturnCycle>();
        foreach (var item in items)
        {
            var derived = CappedAssetReturnCycle.NewCappedAssetReturnCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeCappedAssetReturnCycle(CappedAssetReturnCycle item)
    {
        return TieredWarrantyOverridePolicy.NormalizeTieredWarrantyOverrideCode(item.Code);
    }
}
