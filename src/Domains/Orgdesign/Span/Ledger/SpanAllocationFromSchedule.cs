namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Functions of span allocation from schedule.</summary>
public static class SpanAllocationFromSchedule
{
    public static Dictionary<string, double> SummarizeSpanSchedulesForAllocation(List<SpanSchedule> items)
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

    public static List<SpanAllocation> DeriveSpanAllocationsFromSchedule(List<SpanSchedule> items)
    {
        var outValue = new List<SpanAllocation>();
        foreach (var item in items)
        {
            var derived = SpanAllocation.NewSpanAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreSpanAllocation(SpanAllocation item)
    {
        return DefaultSpanProfilePolicy.DefaultSpanProfileBucket(item.NetTotal());
    }
}
