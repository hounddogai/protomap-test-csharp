namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Functions of default feedback reservation from quota.</summary>
public static class DefaultFeedbackReservationFromQuota
{
    public static Dictionary<string, double> SummarizeFeedbackQuotasForReservation(List<FeedbackQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<DefaultFeedbackReservation> DeriveDefaultFeedbackReservationsFromQuota(List<FeedbackQuota> items)
    {
        var outValue = new List<DefaultFeedbackReservation>();
        foreach (var item in items)
        {
            var derived = DefaultFeedbackReservation.NewDefaultFeedbackReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchDefaultFeedbackReservation(DefaultFeedbackReservation item)
    {
        return FeedbackAllocationPolicy.FeedbackAllocationWeight(1, true);
    }
}
