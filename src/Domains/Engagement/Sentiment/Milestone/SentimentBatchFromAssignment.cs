namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Functions of sentiment batch from assignment.</summary>
public static class SentimentBatchFromAssignment
{
    public static Dictionary<string, double> SummarizeActionPlanAssignmentsForBatch(List<ActionPlanAssignment> items)
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

    public static List<SentimentBatch> DeriveSentimentBatchesFromAssignment(List<ActionPlanAssignment> items)
    {
        var outValue = new List<SentimentBatch>();
        foreach (var item in items)
        {
            var derived = SentimentBatch.NewSentimentBatch(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateSentimentBatch(SentimentBatch item)
    {
        return ActionPlanPreferencePolicy.ActionPlanPreferenceWeight(3, true);
    }
}
