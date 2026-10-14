namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Functions of feedback group from request.</summary>
public static class FeedbackGroupFromRequest
{
    public static Dictionary<string, double> SummarizeSentimentRequestsForGroup(List<SentimentRequest> items)
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

    public static List<FeedbackGroup> DeriveFeedbackGroupsFromRequest(List<SentimentRequest> items)
    {
        var outValue = new List<FeedbackGroup>();
        foreach (var item in items)
        {
            var derived = FeedbackGroup.NewFeedbackGroup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileFeedbackGroup(FeedbackGroup item)
    {
        return SentimentSchedulePolicy.NormalizeSentimentScheduleCode(item.Code);
    }
}
