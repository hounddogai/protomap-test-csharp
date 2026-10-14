namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>Functions of seasonal merit cycle entry from checkpoint.</summary>
public static class SeasonalMeritCycleEntryFromCheckpoint
{
    public static Dictionary<string, double> SummarizeFallbackAdjustmentCheckpointsForEntry(List<FallbackAdjustmentCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<SeasonalMeritCycleEntry> DeriveSeasonalMeritCycleEntriesFromCheckpoint(List<FallbackAdjustmentCheckpoint> items)
    {
        var outValue = new List<SeasonalMeritCycleEntry>();
        foreach (var item in items)
        {
            var derived = SeasonalMeritCycleEntry.NewSeasonalMeritCycleEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignSeasonalMeritCycleEntry(SeasonalMeritCycleEntry item)
    {
        return AdjustmentForecastPolicy.NormalizeAdjustmentForecastCode(item.Code);
    }
}
