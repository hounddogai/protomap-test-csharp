namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Functions of feedback assignment from variance.</summary>
public static class FeedbackAssignmentFromVariance
{
    public static Dictionary<string, double> SummarizePendingSentimentVariancesForAssignment(List<PendingSentimentVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<FeedbackAssignment> DeriveFeedbackAssignmentsFromVariance(List<PendingSentimentVariance> items)
    {
        var outValue = new List<FeedbackAssignment>();
        foreach (var item in items)
        {
            var derived = FeedbackAssignment.NewFeedbackAssignment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyFeedbackAssignment(FeedbackAssignment item)
    {
        return PendingSentimentVariancePolicy.PendingSentimentVarianceBucket(item.WeightedExposure());
    }
}
