namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Functions of span schedule from claim.</summary>
public static class SpanScheduleFromClaim
{
    public static Dictionary<string, double> SummarizeSpanClaimsForSchedule(List<SpanClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<SpanSchedule> DeriveSpanSchedulesFromClaim(List<SpanClaim> items)
    {
        var outValue = new List<SpanSchedule>();
        foreach (var item in items)
        {
            var derived = SpanSchedule.NewSpanSchedule(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeSpanSchedule(SpanSchedule item)
    {
        return ScheduledSpanDigestPolicy.ScheduledSpanDigestBucket(item.ProjectedVolume());
    }
}
