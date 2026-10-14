namespace Acme.Workforce.Compensation.PayBand.Manifest;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Functions of primary pay band manifest from slot.</summary>
public static class PrimaryPayBandManifestFromSlot
{
    public static Dictionary<string, double> SummarizeRegionalEquityGrantSlotsForManifest(List<RegionalEquityGrantSlot> items)
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

    public static List<PrimaryPayBandManifest> DerivePrimaryPayBandManifestsFromSlot(List<RegionalEquityGrantSlot> items)
    {
        var outValue = new List<PrimaryPayBandManifest>();
        foreach (var item in items)
        {
            var derived = PrimaryPayBandManifest.NewPrimaryPayBandManifest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcilePrimaryPayBandManifest(PrimaryPayBandManifest item)
    {
        return ScheduledEquityGrantRollupPolicy.ScheduledEquityGrantRollupWeight(7, true);
    }
}
