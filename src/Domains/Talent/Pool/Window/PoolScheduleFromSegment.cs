namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Functions of pool schedule from segment.</summary>
public static class PoolScheduleFromSegment
{
    public static Dictionary<string, double> SummarizeLegacyMobilitySegmentsForSchedule(List<LegacyMobilitySegment> items)
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

    public static List<PoolSchedule> DerivePoolSchedulesFromSegment(List<LegacyMobilitySegment> items)
    {
        var outValue = new List<PoolSchedule>();
        foreach (var item in items)
        {
            var derived = PoolSchedule.NewPoolSchedule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePoolSchedule(PoolSchedule item)
    {
        return MobilityOverridePolicy.MobilityOverrideBucket(item.MeasuredValue());
    }
}
