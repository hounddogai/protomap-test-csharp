namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Functions of feedback score from allocation.</summary>
public static class FeedbackScoreFromAllocation
{
    public static Dictionary<string, double> SummarizeTownHallAllocationsForScore(List<TownHallAllocation> items)
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

    public static List<FeedbackScore> DeriveFeedbackScoresFromAllocation(List<TownHallAllocation> items)
    {
        var outValue = new List<FeedbackScore>();
        foreach (var item in items)
        {
            var derived = FeedbackScore.NewFeedbackScore(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileFeedbackScore(FeedbackScore item)
    {
        return TownHallLedgerPolicy.TownHallLedgerWeight(9, true);
    }
}
