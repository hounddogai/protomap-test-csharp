namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Functions of sentiment group from milestone.</summary>
public static class SentimentGroupFromMilestone
{
    public static Dictionary<string, double> SummarizeSentimentMilestonesForGroup(List<SentimentMilestone> items)
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

    public static List<SentimentGroup> DeriveSentimentGroupsFromMilestone(List<SentimentMilestone> items)
    {
        var outValue = new List<SentimentGroup>();
        foreach (var item in items)
        {
            var derived = SentimentGroup.NewSentimentGroup(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateSentimentGroup(SentimentGroup item)
    {
        return SentimentLedgerPolicy.ClassifySentimentLedger(item.ProjectedTotal());
    }
}
