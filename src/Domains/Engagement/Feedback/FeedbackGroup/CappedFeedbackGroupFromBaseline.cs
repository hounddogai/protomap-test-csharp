namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Functions of capped feedback group from baseline.</summary>
public static class CappedFeedbackGroupFromBaseline
{
    public static Dictionary<string, double> SummarizeBatchedFeedbackBaselinesForGroup(List<BatchedFeedbackBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<CappedFeedbackGroup> DeriveCappedFeedbackGroupsFromBaseline(List<BatchedFeedbackBaseline> items)
    {
        var outValue = new List<CappedFeedbackGroup>();
        foreach (var item in items)
        {
            var derived = CappedFeedbackGroup.NewCappedFeedbackGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchCappedFeedbackGroup(CappedFeedbackGroup item)
    {
        return BatchedFeedbackBaselinePolicy.BatchedFeedbackBaselineBucket(item.MeasuredValue());
    }
}
