namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Functions of seasonal warranty tier from claim.</summary>
public static class SeasonalWarrantyTierFromClaim
{
    public static Dictionary<string, double> SummarizeRollingLaptopClaimsForTier(List<RollingLaptopClaim> items)
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

    public static List<SeasonalWarrantyTier> DeriveSeasonalWarrantyTiersFromClaim(List<RollingLaptopClaim> items)
    {
        var outValue = new List<SeasonalWarrantyTier>();
        foreach (var item in items)
        {
            var derived = SeasonalWarrantyTier.NewSeasonalWarrantyTier(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveSeasonalWarrantyTier(SeasonalWarrantyTier item)
    {
        return LenientLaptopWindowPolicy.NormalizeLenientLaptopWindowCode(item.Code);
    }
}
