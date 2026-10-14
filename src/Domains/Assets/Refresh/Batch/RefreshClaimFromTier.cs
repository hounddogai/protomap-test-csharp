namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Functions of refresh claim from tier.</summary>
public static class RefreshClaimFromTier
{
    public static Dictionary<string, double> SummarizeTieredLaptopTiersForClaim(List<TieredLaptopTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RefreshClaim> DeriveRefreshClaimsFromTier(List<TieredLaptopTier> items)
    {
        var outValue = new List<RefreshClaim>();
        foreach (var item in items)
        {
            var derived = RefreshClaim.NewRefreshClaim(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateRefreshClaim(RefreshClaim item)
    {
        return TieredLaptopTierPolicy.ClassifyTieredLaptopTier(item.AdjustedValue());
    }
}
