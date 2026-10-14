namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Functions of sentiment milestone from cycle.</summary>
public static class SentimentMilestoneFromCycle
{
    public static Dictionary<string, double> SummarizeTownHallCyclesForMilestone(List<TownHallCycle> items)
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

    public static List<SentimentMilestone> DeriveSentimentMilestonesFromCycle(List<TownHallCycle> items)
    {
        var outValue = new List<SentimentMilestone>();
        foreach (var item in items)
        {
            var derived = SentimentMilestone.NewSentimentMilestone(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitSentimentMilestone(SentimentMilestone item)
    {
        return TownHallRequestPolicy.ClassifyTownHallRequest(item.MeasuredValue());
    }
}
