namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Functions of license seat milestone from tier.</summary>
public static class LicenseSeatMilestoneFromTier
{
    public static Dictionary<string, double> SummarizeTieredLaptopTiersForMilestone(List<TieredLaptopTier> items)
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

    public static List<LicenseSeatMilestone> DeriveLicenseSeatMilestonesFromTier(List<TieredLaptopTier> items)
    {
        var outValue = new List<LicenseSeatMilestone>();
        foreach (var item in items)
        {
            var derived = LicenseSeatMilestone.NewLicenseSeatMilestone(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLicenseSeatMilestone(LicenseSeatMilestone item)
    {
        return LaptopManifestPolicy.LaptopManifestBucket(item.MeasuredValue());
    }
}
