namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Functions of fallback sentiment plan from plan.</summary>
public static class FallbackSentimentPlanFromPlan
{
    public static Dictionary<string, double> SummarizePendingTownHallPlansForPlan(List<PendingTownHallPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<FallbackSentimentPlan> DeriveFallbackSentimentPlansFromPlan(List<PendingTownHallPlan> items)
    {
        var outValue = new List<FallbackSentimentPlan>();
        foreach (var item in items)
        {
            var derived = FallbackSentimentPlan.NewFallbackSentimentPlan(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignFallbackSentimentPlan(FallbackSentimentPlan item)
    {
        return PendingTownHallPlanPolicy.PendingTownHallPlanBucket(item.NetExposure());
    }
}
