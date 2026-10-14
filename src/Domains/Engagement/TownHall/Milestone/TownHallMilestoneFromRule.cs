namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Functions of town hall milestone from rule.</summary>
public static class TownHallMilestoneFromRule
{
    public static Dictionary<string, double> SummarizeSentimentRulesForMilestone(List<SentimentRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<TownHallMilestone> DeriveTownHallMilestonesFromRule(List<SentimentRule> items)
    {
        var outValue = new List<TownHallMilestone>();
        foreach (var item in items)
        {
            var derived = TownHallMilestone.NewTownHallMilestone(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTownHallMilestone(TownHallMilestone item)
    {
        return PendingSentimentDigestPolicy.ClassifyPendingSentimentDigest(item.AdjustedLoad());
    }
}
