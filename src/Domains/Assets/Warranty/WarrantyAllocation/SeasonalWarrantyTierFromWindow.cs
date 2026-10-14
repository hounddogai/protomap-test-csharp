namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Functions of seasonal warranty tier from window.</summary>
public static class SeasonalWarrantyTierFromWindow
{
    public static Dictionary<string, double> SummarizePrimaryLicenseSeatWindowsForTier(List<PrimaryLicenseSeatWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<SeasonalWarrantyTier> DeriveSeasonalWarrantyTiersFromWindow(List<PrimaryLicenseSeatWindow> items)
    {
        var outValue = new List<SeasonalWarrantyTier>();
        foreach (var item in items)
        {
            var derived = SeasonalWarrantyTier.NewSeasonalWarrantyTier(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceSeasonalWarrantyTier(SeasonalWarrantyTier item)
    {
        return LicenseSeatBaselinePolicy.ClassifyLicenseSeatBaseline(item.AdjustedTotal());
    }
}
