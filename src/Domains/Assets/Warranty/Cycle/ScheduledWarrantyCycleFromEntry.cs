namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Functions of scheduled warranty cycle from entry.</summary>
public static class ScheduledWarrantyCycleFromEntry
{
    public static Dictionary<string, double> SummarizeAssetReturnEntriesForCycle(List<AssetReturnEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<ScheduledWarrantyCycle> DeriveScheduledWarrantyCyclesFromEntry(List<AssetReturnEntry> items)
    {
        var outValue = new List<ScheduledWarrantyCycle>();
        foreach (var item in items)
        {
            var derived = ScheduledWarrantyCycle.NewScheduledWarrantyCycle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectScheduledWarrantyCycle(ScheduledWarrantyCycle item)
    {
        return AssetReturnEntryPolicy.ClassifyAssetReturnEntry(item.ProjectedExposure());
    }
}
