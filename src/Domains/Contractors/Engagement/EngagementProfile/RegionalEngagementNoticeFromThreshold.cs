namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Functions of regional engagement notice from threshold.</summary>
public static class RegionalEngagementNoticeFromThreshold
{
    public static Dictionary<string, double> SummarizeStatementOfWorkThresholdsForNotice(List<StatementOfWorkThreshold> items)
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

    public static List<RegionalEngagementNotice> DeriveRegionalEngagementNoticesFromThreshold(List<StatementOfWorkThreshold> items)
    {
        var outValue = new List<RegionalEngagementNotice>();
        foreach (var item in items)
        {
            var derived = RegionalEngagementNotice.NewRegionalEngagementNotice(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeRegionalEngagementNotice(RegionalEngagementNotice item)
    {
        return FallbackStatementOfWorkSegmentPolicy.FallbackStatementOfWorkSegmentBucket(item.AdjustedValue());
    }
}
