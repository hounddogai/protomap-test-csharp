namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Functions of global action plan segment from score.</summary>
public static class GlobalActionPlanSegmentFromScore
{
    public static Dictionary<string, double> SummarizeFeedbackScoresForSegment(List<FeedbackScore> items)
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

    public static List<GlobalActionPlanSegment> DeriveGlobalActionPlanSegmentsFromScore(List<FeedbackScore> items)
    {
        var outValue = new List<GlobalActionPlanSegment>();
        foreach (var item in items)
        {
            var derived = GlobalActionPlanSegment.NewGlobalActionPlanSegment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }
}
