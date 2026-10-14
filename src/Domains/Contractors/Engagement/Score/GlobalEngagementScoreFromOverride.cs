namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Contractors.Engagement.EngagementProfile;

/// <summary>Functions of global engagement score from override.</summary>
public static class GlobalEngagementScoreFromOverride
{
    public static Dictionary<string, double> SummarizeEngagementOverridesForScore(List<EngagementOverride> items)
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

    public static List<GlobalEngagementScore> DeriveGlobalEngagementScoresFromOverride(List<EngagementOverride> items)
    {
        var outValue = new List<GlobalEngagementScore>();
        foreach (var item in items)
        {
            var derived = GlobalEngagementScore.NewGlobalEngagementScore(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyGlobalEngagementScore(GlobalEngagementScore item)
    {
        return FallbackEngagementProfilePolicy.FallbackEngagementProfileBucket(item.WeightedExposure());
    }
}
