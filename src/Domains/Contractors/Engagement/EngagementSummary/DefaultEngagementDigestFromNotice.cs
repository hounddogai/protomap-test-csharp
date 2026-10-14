namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Functions of default engagement digest from notice.</summary>
public static class DefaultEngagementDigestFromNotice
{
    public static Dictionary<string, double> SummarizeScheduledEngagementNoticesForDigest(List<ScheduledEngagementNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<DefaultEngagementDigest> DeriveDefaultEngagementDigestsFromNotice(List<ScheduledEngagementNotice> items)
    {
        var outValue = new List<DefaultEngagementDigest>();
        foreach (var item in items)
        {
            var derived = DefaultEngagementDigest.NewDefaultEngagementDigest(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchDefaultEngagementDigest(DefaultEngagementDigest item)
    {
        return BatchedEngagementExceptionPolicy.NormalizeBatchedEngagementExceptionCode(item.Code);
    }
}
