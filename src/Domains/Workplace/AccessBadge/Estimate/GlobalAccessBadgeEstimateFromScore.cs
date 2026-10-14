namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Functions of global access badge estimate from score.</summary>
public static class GlobalAccessBadgeEstimateFromScore
{
    public static Dictionary<string, double> SummarizeAccessBadgeScoresForEstimate(List<AccessBadgeScore> items)
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

    public static List<GlobalAccessBadgeEstimate> DeriveGlobalAccessBadgeEstimatesFromScore(List<AccessBadgeScore> items)
    {
        var outValue = new List<GlobalAccessBadgeEstimate>();
        foreach (var item in items)
        {
            var derived = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyGlobalAccessBadgeEstimate(GlobalAccessBadgeEstimate item)
    {
        return AccessBadgeScorePolicy.ClassifyAccessBadgeScore(item.EffectiveVolume());
    }
}
