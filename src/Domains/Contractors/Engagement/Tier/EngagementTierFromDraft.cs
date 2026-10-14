namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Functions of engagement tier from draft.</summary>
public static class EngagementTierFromDraft
{
    public static Dictionary<string, double> SummarizeRateDraftsForTier(List<RateDraft> items)
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

    public static List<EngagementTier> DeriveEngagementTiersFromDraft(List<RateDraft> items)
    {
        var outValue = new List<EngagementTier>();
        foreach (var item in items)
        {
            var derived = EngagementTier.NewEngagementTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyEngagementTier(EngagementTier item)
    {
        return RateQuotaPolicy.ClassifyRateQuota(item.NetExposure());
    }
}
