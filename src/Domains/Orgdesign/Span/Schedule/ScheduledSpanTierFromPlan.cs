namespace Acme.Workforce.Orgdesign.Span.Schedule;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Functions of scheduled span tier from plan.</summary>
public static class ScheduledSpanTierFromPlan
{
    public static Dictionary<string, double> SummarizeLegacyPositionPlansForTier(List<LegacyPositionPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<ScheduledSpanTier> DeriveScheduledSpanTiersFromPlan(List<LegacyPositionPlan> items)
    {
        var outValue = new List<ScheduledSpanTier>();
        foreach (var item in items)
        {
            var derived = ScheduledSpanTier.NewScheduledSpanTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectScheduledSpanTier(ScheduledSpanTier item)
    {
        return RollingPositionManifestPolicy.RollingPositionManifestBucket(item.EffectiveExposure());
    }
}
