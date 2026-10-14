namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Functions of scheduled refresh cycle from manifest.</summary>
public static class ScheduledRefreshCycleFromManifest
{
    public static Dictionary<string, double> SummarizeRefreshManifestsForCycle(List<RefreshManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<ScheduledRefreshCycle> DeriveScheduledRefreshCyclesFromManifest(List<RefreshManifest> items)
    {
        var outValue = new List<ScheduledRefreshCycle>();
        foreach (var item in items)
        {
            var derived = ScheduledRefreshCycle.NewScheduledRefreshCycle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanScheduledRefreshCycle(ScheduledRefreshCycle item)
    {
        return RefreshManifestPolicy.ClassifyRefreshManifest(item.EffectiveValue());
    }
}
