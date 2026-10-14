namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.Engagement.Policy;

/// <summary>Functions of statement of work metric from segment.</summary>
public static class StatementOfWorkMetricFromSegment
{
    public static Dictionary<string, double> SummarizeAdaptiveEngagementSegmentsForMetric(List<AdaptiveEngagementSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<StatementOfWorkMetric> DeriveStatementOfWorkMetricsFromSegment(List<AdaptiveEngagementSegment> items)
    {
        var outValue = new List<StatementOfWorkMetric>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkMetric.NewStatementOfWorkMetric(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankStatementOfWorkMetric(StatementOfWorkMetric item)
    {
        return AdaptiveEngagementSegmentPolicy.AdaptiveEngagementSegmentBucket(item.NetExposure());
    }
}
