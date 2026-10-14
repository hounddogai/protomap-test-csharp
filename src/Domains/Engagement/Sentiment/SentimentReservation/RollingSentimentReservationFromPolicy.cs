namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Functions of rolling sentiment reservation from policy.</summary>
public static class RollingSentimentReservationFromPolicy
{
    public static Dictionary<string, double> SummarizeFeedbackPoliciesForReservation(List<FeedbackPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<RollingSentimentReservation> DeriveRollingSentimentReservationsFromPolicy(List<FeedbackPolicy> items)
    {
        var outValue = new List<RollingSentimentReservation>();
        foreach (var item in items)
        {
            var derived = RollingSentimentReservation.NewRollingSentimentReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignRollingSentimentReservation(RollingSentimentReservation item)
    {
        return FeedbackAllocationPolicy.FeedbackAllocationBucket(item.MeasuredValue());
    }
}
