namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Functions of scheduled span tier from threshold.</summary>
public static class ScheduledSpanTierFromThreshold
{
    public static Dictionary<string, double> SummarizeScenarioThresholdsForTier(List<ScenarioThreshold> items)
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

    public static List<ScheduledSpanTier> DeriveScheduledSpanTiersFromThreshold(List<ScenarioThreshold> items)
    {
        var outValue = new List<ScheduledSpanTier>();
        foreach (var item in items)
        {
            var derived = ScheduledSpanTier.NewScheduledSpanTier(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveScheduledSpanTier(ScheduledSpanTier item)
    {
        return SeasonalScenarioRequestPolicy.ClassifySeasonalScenarioRequest(item.EffectiveExposure());
    }
}
